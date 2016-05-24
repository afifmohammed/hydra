using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hydra.Core;

namespace Hydra.RequestPipeline
{
    public interface IRequest<TResult>        
    { }

    public sealed class Unit { }
        
    public class Authenticate<TRequest, TResult> : IRequest<bool> 
        where TRequest : IRequest<TResult>
    {
        public TRequest Request { get; set; }        
    }

    public class Authorise<TRequest, TResult> : IRequest<bool>
        where TRequest : IRequest<TResult>
    {
        public TRequest Request { get; set; }
    }

    public class Validate<TRequest, TResult> : IRequest<IEnumerable<KeyValuePair<string, string>>>
        where TRequest : IRequest<TResult>
    {
        public TRequest Request { get; set; }
    }

    public class Placed<TCommand> : IDomainEvent where TCommand : IRequest<Unit>, ICorrelated
    {
        public TCommand Command { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => Command.Correlations;
        public DateTimeOffset? When { get; set; }
    }

    public class Response<TResult> : ICorrelated
    {
        public Response()
        {
            Result = Enumerable.Empty<TResult>();
            ValidationMessages = new List<KeyValuePair<string, string>>();
            Correlations = new List<KeyValuePair<string, object>>();
        }

        public HttpStatusCode Status { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ValidationMessages { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations { get; set; }

        public IEnumerable<TResult> Result { get; set; } 
    }
}
