using System;
using System.Collections.Generic;
using System.Linq;

namespace Requests
{
    public interface IRequest<TResult> where TResult : class
    {}

    public delegate IEnumerable<TResult> RequestHandler<TResult>(IRequest<TResult> input) where TResult : class;
    
    public static class Request<TResult> where TResult : class
    {
        public static RequestHandler<TResult> By = 
            input => RequestHandlers.Routes
                .Where(r => r.Key.Equals(Contract(input)))
                .SelectMany(r => ((Func<object, IEnumerable<TResult>>)(r.Value))(input));

        static FunctionContract Contract(IRequest<TResult> input)
        {
            return new FunctionContract(input.Contract(), typeof(TResult).Contract());
        }        
    }

}
