using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.RequestPipeline;
using Hydra.Requests;

namespace Hydra.Subscriptions
{
    public static class PostBox<TQueueProvider>
        where TQueueProvider : IProvider
    {
        public static Enqueue<TQueueProvider> Enqueue { get; set; }
        public static CommitWork<TQueueProvider> CommitWork { get; set; }

        public static Notify Drop =
            getSubscriptions =>
                notifications =>
                    Post(notifications.SelectMany(notification => notification.SubscriberMessages(getSubscriptions())));

        public static Post Post = messages => CommitWork(provider => Enqueue(provider, messages));
    }

    public class SubscriptionDispatcher<TQueueProvider> where TQueueProvider : IProvider
    {
        public static Func<IEnumerable<Subscription>> GetSubscriptions = () => Request<Subscription>.By(new RegisteredSubscriptions());

        public static Response<Unit> Dispatch<TRequest>(TRequest command) where TRequest : IRequest<Unit>, ICorrelated
        {
            var subscriptions = new Lazy<IReadOnlyCollection<Subscription>>(() => new List<Subscription>(GetSubscriptions()).AsReadOnly());

            return RequestPipeline<TRequest, Unit>.DispatchThroughPipeline(
                command,
                dispatcher: input =>
                {
                    var @event = new Event
                    {
                        Notification = new Placed<TRequest> { Command = input },
                        EventId = new NoEventId()
                    };

                    PostBox<TQueueProvider>.Drop(() => subscriptions.Value)(new[] { @event });

                    return Enumerable.Empty<Unit>();
                });
        }
    }
}