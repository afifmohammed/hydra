using EventSourcing;
using System.Collections.Generic;
using System.Net;
using Requests;

namespace Commands
{
    public class Authenticate : IRequest<Unit<bool>>
    {
        public ICommand Command { get; set; }
    }

    public class Authorise : IRequest<Unit<bool>>
    {
        public ICommand Command { get; set; }
    }

    public class Validate : IRequest<IEnumerable<KeyValuePair<string, string>>>
    {
        public ICommand Command { get; set; }
    }

    public interface ICommand : ICorrelated { }

    public class Received<TCommand> : IDomainEvent where TCommand : ICommand
    {
        public TCommand Command { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => Command.Correlations;
    }

    public class Response : ICorrelated
    {
        public Response()
        {
            ValidationMessages = new List<KeyValuePair<string, string>>();
            Correlations = new List<KeyValuePair<string, object>>();
        }
        public HttpStatusCode Status { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ValidationMessages { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations { get; set; }
    }
}
