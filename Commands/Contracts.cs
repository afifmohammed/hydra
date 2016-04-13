using EventSourcing;
using System.Collections.Generic;

namespace Commands
{
    public interface ICommand : ICorrelated { }

    public class Received<TCommand> : IDomainEvent where TCommand : ICommand
    {
        public TCommand Command { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => Command.Correlations;
    }
}
