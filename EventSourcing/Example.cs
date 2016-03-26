using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
            new Dictionary<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>>
            {
                {
                    typeof(BankAccountNominated).Contract(),
                    e => new []
                    {
                        Correlation.Property(x => x.ApplicationId, (BankAccountNominated)e),
                    }
                },
                {
                    typeof(BankAccountsNominated).Contract(),
                    e => new []
                    {
                        Correlation.Property(x => x.ApplicationId, (BankAccountsNominated)e),
                    }
                },
                {
                    typeof(BankAccountRetreived).Contract(),
                    e => new []
                    {
                        Correlation.Property(x => x.AccountId, (BankAccountRetreived)e),
                        Correlation.Property(x => x.LoginId, (BankAccountRetreived)e),
                        Correlation.Property(x => x.ApplicationId, (BankAccountRetreived)e),
                    }
                },
                {
                    typeof(NominatedBankAccountMatched).Contract(),
                    e => new [] 
                    {
                        Correlation.Property(x => x.ApplicationId, (NominatedBankAccountMatched)e),
                    }
                },
                {
                    typeof(NominatedBankAccountNotMatched).Contract(),
                    e => new []
                    {
                        Correlation.Property(x => x.ApplicationId, (NominatedBankAccountNotMatched)e),
                    }
                },
                {
                    typeof(BankLoginReceived).Contract(),
                    e => new []
                    {
                        Correlation.Property(x => x.LoginId, (BankLoginReceived)e),
                        Correlation.Property(x => x.ApplicationId, (BankLoginReceived)e),
                    }
                },
                {
                    typeof(BankLoginIncorrect).Contract(),
                    e => new []
                    {
                        Correlation.Property(x => x.LoginId, (BankLoginIncorrect)e),
                        Correlation.Property(x => x.ApplicationId, (BankLoginIncorrect)e),
                    }
                },
                // todo: add more
            };

        readonly IDictionary<TypeContract, Func<MatchNominatedBankAccount, JsonContent, MatchNominatedBankAccount>> publisherDataMappers = 
            new Dictionary<TypeContract, Func<MatchNominatedBankAccount, JsonContent, MatchNominatedBankAccount>>
            {
                {
                    typeof(BankLoginReceived).Contract(),
                    (d,json) =>
                    {
                        var notification = JsonConvert.DeserializeObject<BankLoginReceived>(json.Value);
                        return new MatchNominatedBankAccount
                        {
                            PendingLogins = (d.PendingLogins ?? new List<string>()).With(l => l.Add(notification.LoginId)),
                            ApplicationId = d.ApplicationId
                        };
                    }
                },
                // todo: add more
            };

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
            // todo: asserts
        }        
    }
}

