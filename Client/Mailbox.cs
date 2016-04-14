using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using EventSourcing;

namespace Client
{
    public delegate void Notify(IDomainEvent notification);

    public delegate void NotifyViaPost(
        IDomainEvent notification,
        SubscriberMessagesByNotification subscriberMessagesByNotification,
        Post post);

    public delegate IEnumerable<SubscriberMessage> SubscriberMessagesByNotification(IDomainEvent notification);

    public delegate void Post(IEnumerable<SubscriberMessage> messages);

    delegate void Route(SubscriberMessage message);

    public static class Mailbox
    {
        public static SubscriberMessagesByNotification SubscriberMessagesByNotification = notification =>
            new List<SubscriberMessage>()
                    .With(x => x.AddRange(Channel.PrepareMessages(notification, EventStore.PublishersBySubscription()).Cast<SubscriberMessage>()))
                    .With(x => x.AddRange(Channel<AdoNetViewStoreConnection>.PrepareMessages(notification, EventStore<AdoNetViewStoreConnection>.ConsumersBySubscription).Cast<SubscriberMessage>()));

        public static Notify Notify = notification => 
            NotifyViaPost
            (
                notification, 
                SubscriberMessagesByNotification, 
                Post
            );

        public static NotifyViaPost NotifyViaPost = 
        (
            notification, 
            subscriberMessagesByNotification, 
            post
        ) =>
            post(subscriberMessagesByNotification(notification));

        public static Post Post = messages =>
            {
                using (var transaction = new TransactionScope())
                {
                    foreach (var message in messages)
                    {
                        Hangfire.BackgroundJob.Enqueue(() => Route(message));
                    }

                    transaction.Complete();
                }
            };

        static Route Route = message =>
            {
                var routes = new Dictionary<TypeContract, Action<SubscriberMessage>>
                {
                    {
                        typeof (MessageToPublisher).Contract(),
                        m => EventStore.Commit((MessageToPublisher) m)
                    },
                    {
                        typeof (MessageToConsumer<AdoNetViewStoreConnection>).Contract(),
                        m => AdoNetViewStore.Post((MessageToConsumer<AdoNetViewStoreConnection>) m)
                    }
                };

                routes[message.Contract()](message);
            };
    }
}