using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;
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

    public static class RequestPipeline<TProvider> where TProvider : IProvider
    {
        public static Func<TRequest, Response<Unit>> Dispatch<TRequest>(Func<IEnumerable<Subscription>> getSubscriptions) where TRequest : IRequest<Unit>, ICorrelated => 
            input => RequestPipeline<TRequest, Unit>.DispatchThroughPipeline(
                input,
                request =>
                {
                    PostBox<TProvider>.Drop(getSubscriptions)(new [] { new Placed<TRequest> { Command = request } });
                    return Enumerable.Empty<Unit>();
                });
    }
}
