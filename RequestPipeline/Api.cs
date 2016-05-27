using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;
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

    public static class RequestPipeline<TProvider> where TProvider : IProvider
    {
        public static Func<TRequest, Response<Unit>> Place<TRequest>(Func<IEnumerable<Subscription>> getSubscriptions)
            where TRequest : IRequest<Unit>, ICorrelated
        {
            return request => RequestPipeline<TRequest, Unit>.DispatchThroughPipeline(
                request,
                dispatcher:input =>
                {
                    var @event = new Event
                    {
                        Notification = new Placed<TRequest> {Command = input},
                        EventId = new NoEventId()
                    };

                    PostBox<TProvider>.Drop(getSubscriptions)(new[] { @event });

                    return Enumerable.Empty<Unit>();
                });
        }
    }
}
