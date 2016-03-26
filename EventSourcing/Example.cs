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
        public string AccountId { get; set; }
        public string ApplicationId { get; set; }
    }

    public class NominatedBankAccountNotMatched : IDomainEvent
    {
        public string Bsb { get; set; }
        public string Acc { get; set; }
        public string Name { get; set; }
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
            yield return new JustSpinningMyWheels();
        }

        public static IEnumerable<IDomainEvent> On(BankAccountsNominated e, MatchNominatedBankAccount data)
        {
            yield return new JustSpinningMyWheels();
        }

        public static IEnumerable<IDomainEvent> On(BankAccountNominated e, MatchNominatedBankAccount data)
        {
            yield return new JustSpinningMyWheels();
        }
    }

    class Tests
    {
        [Fact]
        public void WorksOutOfTheBox()
        {
            var correlationMaps = new Dictionary<TypeContract, IEnumerable<CorrelationMap>>
            {
                {
                    typeof(MatchNominatedBankAccount).Contract(),
                    new List<CorrelationMap>
                    {
                        CorrelationMap.Between<MatchNominatedBankAccount, BankLoginReceived>(x => x.ApplicationId, x => x.ApplicationId),                        
                    }
                }
            };

            var mappers = new Dictionary<TypeContract, Func<MatchNominatedBankAccount, JsonContent, MatchNominatedBankAccount>>
            {
                {
                    typeof(BankLoginReceived).Contract(),
                    (d,json) =>
                    {
                        var notification = JsonConvert.DeserializeObject<BankLoginReceived>(json.Value);
                        return new MatchNominatedBankAccount
                        {
                            PendingLogins = d.PendingLogins.With(l => l.Add(notification.LoginId)),
                            ApplicationId = d.ApplicationId
                        };
                    }
                }
            };

            var correlationsByNotificationContract = new Dictionary<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>>
            {
                {
                    typeof(NominatedBankAccountMatched).Contract(),
                    e => new [] 
                    {
                        Correlation.Property(x => x.AccountId, (NominatedBankAccountMatched)e),
                        Correlation.Property(x => x.ApplicationId, (NominatedBankAccountMatched)e),
                    }
                }
            };

            var notificationsByCorrelations = new IDomainEvent[] 
            {
                new BankLoginReceived(), new BankAccountRetreived(), new BankAccountsNominated()
            }.Select(n => new
            {
                Notification = new SerializedNotification
                {
                    Contract = n.Contract(),
                    JsonContent = new JsonContent(n)
                },
                Correlations = correlationsByNotificationContract[n.Contract()](n)
            });

            Functions.GroupNotificationsByPublisher<MatchNominatedBankAccount, BankAccountRetreived>
            (
                (e, d) => MatchNominatedBankAccountHandler.On(d, e),
                correlationMaps,
                correlations => notificationsByCorrelations.Where(x => correlations.All(c => x.Correlations.Any(xc => xc.Equals(c)))).Select(x => x.Notification),
                correlationsByNotificationContract,
                mappers          
            );
            
        }
    }
}

