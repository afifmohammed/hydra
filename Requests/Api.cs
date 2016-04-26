using System;
using System.Collections.Generic;
using System.Linq;

namespace Requests
{
    public delegate IEnumerable<TResult> RequestHandler<out TResult>(object request) where TResult : class;
    
    public static class Request<TResult> where TResult : class
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

    public interface IRequest<TResult> where TResult : class
    { }

    public static class Request
    {
        public static IEnumerable<TResult> For<TResult>(IRequest<TResult> request) where TResult : class
        {
            return Request<TResult>.By(request);
        }
    }
}
