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

        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    public class DepositAccountNominated : IDomainEvent
    {
        public string Bsb { get; set; }
        public string Acc { get; set; }
        public string Name { get; set; }
        public string ApplicationId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    public class NominatedDepositAccountMatched : IDomainEvent
    {
        public string ApplicationId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    public class NominatedDepositAccountNotMatched : IDomainEvent
    {
        public string ApplicationId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    /// <summary>
    /// Captures the fact that the user has finished providing logins
    /// </summary>
    public class BankLoginsNominated : IDomainEvent
    {
        public string ApplicationId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    public class BankAccountRetreived : IDomainEvent
    {
        public string LoginId { get; set; }
        public string AccountId { get; set; }
        public string ApplicationId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    public class BankLoginIncorrect : IDomainEvent
    {
        public string LoginId { get; set; }
        public string Bank { get; set; }        
        public string ApplicationId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    public struct MatchNominatedDespositAccount
    {
        public List<string> PendingLogins { get; set; }
        public string ApplicationId { get; set; }
    }

    public class JustSpinningMyWheels : IDomainEvent
    {
        public IEnumerable<KeyValuePair<string, object>> Correlations
        {
            get
            {
                yield return new KeyValuePair<string, object>();
            }
        }
    }

    static class MatchNominatedDepositAccountHandler
    {
        public static IEnumerable<IDomainEvent> On(BankAccountRetreived e, MatchNominatedDespositAccount data)
        {
            yield return new NominatedDepositAccountMatched { ApplicationId = e.ApplicationId };
        }

        public static IEnumerable<IDomainEvent> On(BankLoginsNominated e, MatchNominatedDespositAccount data)
        {
            yield return new NominatedDepositAccountMatched { ApplicationId = e.ApplicationId };
        }

        public static IEnumerable<IDomainEvent> On(DepositAccountNominated e, MatchNominatedDespositAccount data)
        {
            yield return new NominatedDepositAccountMatched { ApplicationId = e.ApplicationId };
        }
    }

    public class PublisherTests
    {
        /*
        new UseCase<MatchNominatedDespositAccount>()
            .Given<BankLoginReceived>(e => d => {})
                .Correlates(e => e.ApplicationId, d => d.ApplicationId)            
            .When<BankAccountRetrieved>(e => d => {})
                .Then((e,d) => MatchNominatedBankAccountHandler.On(e,d))
            .When<BankLoginsNominated>(e => d => {})
                .Then((e,d) => MatchNominatedBankAccountHandler.On(e,d))
            .When<DepositAccountNominated>(e => d => {})
                .Then((e,d) => MatchNominatedBankAccountHandler.On(e,d));
        */

        static readonly IDictionary<TypeContract, IEnumerable<CorrelationMap>> CorrelationMapsByPublisherDataContract =
            new KeyValuePair<TypeContract, CorrelationMap>[]
            {
                Type<MatchNominatedDespositAccount>.Correlates<BankLoginReceived>(d => d.ApplicationId, e => e.ApplicationId),
                Type<MatchNominatedDespositAccount>.Correlates<BankAccountRetreived>(d => d.ApplicationId, e => e.ApplicationId),
                Type<MatchNominatedDespositAccount>.Correlates<BankLoginIncorrect>(d => d.ApplicationId, e => e.ApplicationId)
            }
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Select(a => a.Value));

        static readonly IDictionary<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>> CorrelationsByNotificationContract =
            new KeyValuePair<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>>[]
            {
                Type<DepositAccountNominated>.Correlation(x => x.ApplicationId),
                Type<BankLoginsNominated>.Correlation(x => x.ApplicationId),
                Type<BankAccountRetreived>.Correlation(x => x.ApplicationId, x => x.LoginId, x => x.AccountId),
                Type<NominatedDepositAccountMatched>.Correlation(x => x.ApplicationId),
                Type<NominatedDepositAccountNotMatched>.Correlation(x => x.ApplicationId),
                Type<BankLoginReceived>.Correlation(x => x.ApplicationId, x => x.LoginId),
                Type<BankLoginIncorrect>.Correlation(x => x.ApplicationId, x => x.LoginId),
            }.ToDictionary(x => x.Key, x => x.Value);

        static readonly IDictionary<TypeContract, Func<MatchNominatedDespositAccount, JsonContent, MatchNominatedDespositAccount>> PublisherDataMappers =
            new KeyValuePair<TypeContract, Func<MatchNominatedDespositAccount, JsonContent, MatchNominatedDespositAccount>>[]
            {
                Type<MatchNominatedDespositAccount>.Maps<BankLoginReceived>(e => d => d.PendingLogins?.Add(e.LoginId))
            }.ToDictionary(x => x.Key, x => x.Value);            

        /// <summary>
        /// mimics the behavior of a query that goes to the database 
        /// </summary>        
        static Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> NotificationsByCorrelations(params IDomainEvent[] notifications)
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
                new BankLoginsNominated {ApplicationId = "1"}
            };

            var time = new DateTimeOffset(new DateTime(2012,10,10));

            var publisher = Functions.GroupNotificationsByPublisher<MatchNominatedDespositAccount, BankAccountRetreived>
            (
                (e, d) => MatchNominatedDepositAccountHandler.On(d, e),
                CorrelationMapsByPublisherDataContract,
                NotificationsByCorrelations(notifications),
                n => CorrelationsByNotificationContract[new TypeContract(n)](n),
                PublisherDataMappers,
                () => time
            );

            var notificationsByPublisher = publisher(new BankAccountRetreived { ApplicationId = "1", AccountId = "A1", LoginId = "L1" });
            
            Assert.NotEmpty(notificationsByPublisher.Notifications);
            Assert.NotEmpty(notificationsByPublisher.PublisherDataCorrelations);
            Assert.Equal(time, notificationsByPublisher.When);
        }        
    }
}

