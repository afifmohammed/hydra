using System.Collections.Generic;
using Requests;

namespace RequestPipeline
{
    public static class Request
    {
        public static IEnumerable<TResult> For<TResult>(IRequest<TResult> request) where TResult : class
        {
            return Request<TResult>.By(request);
        }
    }
}
