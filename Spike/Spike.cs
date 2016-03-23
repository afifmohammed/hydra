using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Spike
{
    interface INotification { }
    class ApplicationId : CorrelationMapping<VerifyNominatedAccount>
    {
        public ApplicationId() : base(x => x.ApplicationId)
        {}
    }

    class VerifyNominatedAccount
    {
        public string ApplicationId { get; set; }
        public IDictionary<string, LoginOutcome> Logins { get; set; }
        public List<string> Accounts { get; set; }
        public string NominatedDepositAccountId { get; set; }
        public DateTimeOffset? LoginsNominated { get; set; }
        public bool AccountMatched { get; set; }
    }
    class NominatedAccountMatched : INotification { }
    class BankAccountRetreived : INotification { }
    class BankAccountNominated : INotification { }
    class LoginFailed : INotification
    {
        public string LoginId { get; set; }
        public string ApplicationId { get; set; }
        public DateTimeOffset When { get; set; }
        public string[] Correlation => new[] { LoginId, ApplicationId };
    }

    class LoginReceived : INotification
    {
        public string ApplicationId { get; set; }
    }

    static class VerifyAccountNominatedWhen
    {
        public static VerifyNominatedAccount BankAccountRetreived(VerifyNominatedAccount state, BankAccountRetreived e)
        {
            return new VerifyNominatedAccount();
        }

        public static VerifyNominatedAccount BankAccountNominated(VerifyNominatedAccount state, BankAccountNominated e)
        {
            return new VerifyNominatedAccount();
        }
    }

    

    static class VerifyAccountNominationGiven
    {
        public static VerifyNominatedAccount LoginReceived(VerifyNominatedAccount state, LoginReceived e)
        {
            return new VerifyNominatedAccount();
        }

        public static VerifyNominatedAccount LoginFailed(VerifyNominatedAccount state, LoginFailed e)
        {
            return new VerifyNominatedAccount();
        }

        public static VerifyNominatedAccount BankAccountNominated(VerifyNominatedAccount state, BankAccountNominated e)
        {
            return new VerifyNominatedAccount();
        }
    }

    class CorrelationMapping<TState>
    {
        readonly Expression<Func<TState, dynamic>> _property;
        readonly IDictionary<string, string> _maps;
        public CorrelationMapping(Expression<Func<TState, dynamic>> property)
        {
            _property = property;
            _maps = new Dictionary<string, string>();
        }

        string Key => "";
        string Value(TState state)
        {
            return _property.Compile()(state).ToString();
        }

        public CorrelationMapping<TState> To<TNotification>(Expression<Func<TNotification, dynamic>> map)
        {
            _maps[typeof(TNotification).Name] = "";
            return this;
        }

        string Map<TNotification>()
        {
            string value;
            return _maps.TryGetValue(typeof(TNotification).Name, out value) ? value : Key;
        }
    }

    class UseCase<TState>
    {
        readonly IDictionary<string, Func<TState, INotification, TState>> _mappers = 
            new Dictionary<string, Func<TState, INotification, TState>>();

        public UseCase<TState> Given<TNotification>(Func<TState, TNotification, TState> mapper)
        {
            _mappers[typeof(TNotification).Name] = (s, n) => mapper(s, (TNotification)n);
            return this;
        }

        public UseCase<TState> When<TNotification>(Func<TState, TNotification, TState> mapper)
        {
            _mappers[typeof(TNotification).Name] = (s, n) => mapper(s, (TNotification)n);
            return this;            
        }

        public UseCase<TState> Then(Func<TState, IEnumerable<INotification>> handler)
        {
            return this;
        }

        public UseCase<TState> CorrelatedBy<TKey>() where TKey : CorrelationMapping<TState>
        {
            return this;
        }

        public UseCase<TState> CorrelatedBy<TKey>(Action<CorrelationMapping<TState>> mapping) where TKey : CorrelationMapping<TState>
        {
            return this;
        }
    }

    class LoginOutcome
    {
        public enum Outcomes
        {
            Pending,
            Failed,
            AccountsReceived
        }
        public string Id { get; set; }
        public Outcomes Outcome { get; set; }
        public DateTimeOffset When { get; set; }
    }

    public class Program
    {
        public void Init()
        {
            new UseCase<VerifyNominatedAccount>()                
                .CorrelatedBy<ApplicationId>()
                .Given<LoginFailed>((s,notification) =>
                {
                    var details = s;
                    LoginOutcome loginOutcome;

                    if (!details.Logins.TryGetValue(notification.LoginId, out loginOutcome)) loginOutcome = new LoginOutcome { Id = notification.LoginId };
                    if (notification.When > loginOutcome.When && loginOutcome.Outcome != LoginOutcome.Outcomes.AccountsReceived)
                    {
                        loginOutcome.Outcome = LoginOutcome.Outcomes.Failed;
                        loginOutcome.When = notification.When;

                        details.Logins[loginOutcome.Id] = loginOutcome;
                    }
                    details.ApplicationId = notification.ApplicationId;
                    return details;
                })
                .Given<LoginReceived>(VerifyAccountNominationGiven.LoginReceived)
                .Given<BankAccountNominated>(VerifyAccountNominationGiven.BankAccountNominated)

                .When<BankAccountNominated>(VerifyAccountNominatedWhen.BankAccountNominated)               

                .When<BankAccountRetreived>(VerifyAccountNominatedWhen.BankAccountRetreived)
              
                .Then(s => new[] { new NominatedAccountMatched() });
        }
    }
    

    /*
    given
      LoginFailed
      LoginReceived
      BankAccountNominated
    when
      BankAccountNominated
      BankAccountRetreived
    then
      NominatedAccountMatched
    */

}
