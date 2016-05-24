using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydra.Requests
{
    public delegate IEnumerable<TResult> RequestHandler<out TResult>(object request);
    
    public static class Request<TResult>
    {
        public static RequestHandler<TResult> By =
            request => RequestHandlers.Routes
                .Where(r => r.Key.Equals(Contract(request)))
                .SelectMany(r => ((Func<object, IEnumerable<TResult>>)(r.Value))(request));

        static FunctionContract Contract(object request)
        {
            return new FunctionContract(request.Contract(), typeof(TResult).Contract());
        }        
    }    
}
