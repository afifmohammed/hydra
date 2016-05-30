using System.Collections.Generic;
using Hydra.Requests;

namespace Hydra.RequestPipeline
{
    public static class Request
    {
        public static IEnumerable<TResult> For<TResult>(IRequest<TResult> request) where TResult : class
        {
            return Request<TResult>.By(request);
        }
    }
}