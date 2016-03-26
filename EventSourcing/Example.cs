using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EventSourcing
{
    public class BankLoginReceived : IDomainEvent
    {
        public string LoginId { get; set; }
        public string Bank { get; set; }
        public string Token { get; set; }
        public string ApplicationId { get; set; }
    }

    public class BankAccountNominated : IDomainEvent
    {
        public string Bsb { get; set; }
        public string Acc { get; set; }
        public string Name { get; set; }
        public string ApplicationId { get; set; }
    }

    public class NominatedBankAccountMatched : IDomainEvent
    {
        public string ApplicationId { get; set; }
    }

    public class NominatedBankAccountNotMatched : IDomainEvent
    {
        public string ApplicationId { get; set; }
    }

    public class BankAccountsNominated : IDomainEvent
    {
        public string ApplicationId { get; set; }
    }

    public class BankAccountRetreived : IDomainEvent
    {
        public string LoginId { get; set; }
        public string AccountId { get; set; }
        public string ApplicationId { get; set; }
    }

    public class BankLoginIncorrect : IDomainEvent
    {
        public string LoginId { get; set; }
        public string Bank { get; set; }        
        public string ApplicationId { get; set; }
    }

    public struct MatchNominatedBankAccount
    {
        public List<string> PendingLogins { get; set; }
        public string ApplicationId { get; set; }
    }

    public class JustSpinningMyWheels : IDomainEvent
    { }

    static class MatchNominatedBankAccountHandler
    {
        public static IEnumerable<IDomainEvent> On(BankAccountRetreived e, MatchNominatedBankAccount data)
        {
            yield return new NominatedBankAccountMatched { ApplicationId = e.ApplicationId };
        }

        public static IEnumerable<IDomainEvent> On(BankAccountsNominated e, MatchNominatedBankAccount data)
        {
            yield return new NominatedBankAccountMatched { ApplicationId = e.ApplicationId };
        }

        public static IEnumerable<IDomainEvent> On(BankAccountNominated e, MatchNominatedBankAccount data)
        {
            yield return new NominatedBankAccountMatched { ApplicationId = e.ApplicationId };
        }
    }

    public class Tests
    {
        readonly IDictionary<TypeContract, IEnumerable<CorrelationMap>> correlationMapsByPublisherDataContract = 
            new Dictionary<TypeContract, IEnumerable<CorrelationMap>>
            {
                {
                    typeof(MatchNominatedBankAccount).Contract(),
                    new List<CorrelationMap>
                    {
                        CorrelationMap.Between<MatchNominatedBankAccount, BankLoginReceived>(x => x.ApplicationId, x => x.ApplicationId)                        
                    }
                }                
            };

        readonly IDictionary<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>> CorrelationsByNotificationContract =
            new KeyValuePair<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>>[]
            {
                Type<BankAccountNominated>.Correlation(x => x.ApplicationId),
                Type<BankAccountsNominated>.Correlation(x => x.ApplicationId),
                Type<BankAccountRetreived>.Correlation(x => x.ApplicationId, x => x.LoginId, x => x.AccountId),
                Type<NominatedBankAccountMatched>.Correlation(x => x.ApplicationId),
                Type<NominatedBankAccountNotMatched>.Correlation(x => x.ApplicationId),
                Type<BankLoginReceived>.Correlation(x => x.ApplicationId, x => x.LoginId),
                Type<BankLoginIncorrect>.Correlation(x => x.ApplicationId, x => x.LoginId),
            }.ToDictionary(x => x.Key, x => x.Value);

        readonly IDictionary<TypeContract, Func<MatchNominatedBankAccount, JsonContent, MatchNominatedBankAccount>> publisherDataMappers =
            new KeyValuePair<TypeContract, Func<MatchNominatedBankAccount, JsonContent, MatchNominatedBankAccount>>[]
            {
                Type<MatchNominatedBankAccount>.Maps<BankLoginReceived>(e => d => d.PendingLogins?.Add(e.LoginId))
            }.ToDictionary(x => x.Key, x => x.Value);            

        /// <summary>
        /// mimics the behavior of a query that goes to the database 
        /// </summary>        
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> NotificationsByCorrelations(params IDomainEvent[] notifications)
        {
            return correlations => notifications
                .Select(n => new
                {
                    Notification = new SerializedNotification
                    {
                        Contract = n.Contract(),
                        JsonContent = new JsonContent(n)
                    },
                    Correlations = CorrelationsByNotificationContract[n.Contract()](n)
                })
                .Where(n => correlations.All(c => n.Correlations.Any(nc => nc.Equals(c))))
                .Select(x => x.Notification);
        }

        [Fact]
        public void WorksOutOfTheBox()
        {
            var notifications = new IDomainEvent[]
            {
                new BankLoginReceived { ApplicationId = "1", Bank = "CBA", LoginId = "L1", Token = "T1" },
                new BankAccountsNominated {ApplicationId = "1"}
            };
            
            var publisher = Functions.GroupNotificationsByPublisher<MatchNominatedBankAccount, BankAccountRetreived>
            (
                (e, d) => MatchNominatedBankAccountHandler.On(d, e),
                correlationMapsByPublisherDataContract,
                NotificationsByCorrelations(notifications),
                CorrelationsByNotificationContract,
                publisherDataMappers          
            );

            var notificationsByPublisher = publisher(new BankAccountRetreived { ApplicationId = "1", AccountId = "A1", LoginId = "L1" });
            var list = notificationsByPublisher.Notifications.ToList();

            Assert.NotEmpty(list);
        }        
    }
}

