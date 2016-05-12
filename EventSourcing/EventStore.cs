using System;
using System.Collections.Generic;

namespace EventSourcing
{
    public delegate void Handle(SubscriberMessage message);
    public delegate void NotifyPublisher<in TEndpoint>(MessageToPublisher messageToPublisher, Handler handler, CommitWork<TEndpoint> commitWork) where TEndpoint : class;
    public delegate Func<IEnumerable<Correlation>, int> PublisherVersionByPublisherDataContractCorrelations<in TEndpoint>(TEndpoint connection) where TEndpoint : class;
    public delegate IEnumerable<SerializedNotification> NotificationsByCorrelations(IEnumerable<Correlation> correlation);
    public delegate NotificationsByCorrelations NotificationsByCorrelations<in TEndpoint>(TEndpoint connection) where TEndpoint : class;
    public delegate Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersion<in TEndpoint>(TEndpoint connection) where TEndpoint : class;
    
    public static class EventStore<TPersistence> where TPersistence : class
    {
        public static NotificationsByCorrelations<TPersistence> NotificationsByCorrelations { get; set; }
        public static PublisherVersionByPublisherDataContractCorrelations<TPersistence> PublisherVersionByPublisherDataContractCorrelations { get; set; }
        public static SaveNotificationsByPublisherAndVersion<TPersistence> SaveNotificationsByPublisherAndVersion { get; set; }

        public static Func<Post, NotifyPublisher<TPersistence>> NotifyPublisher = post => 
            (messageToPublisher, handler, commitWork) => NotifyPublisherAndPost(messageToPublisher, handler, commitWork, post);

        public static CommitWork<TPersistence> CommitEventStoreConnection { get; set; }

        public static Func<Post, Handle> Submit = post => message => Handle(message, post);

        private static void NotifyPublisherAndPost(MessageToPublisher messageToPublisher, Handler handler, CommitWork<TPersistence> commitWork, Post post)
        {
            commitWork(connection =>
            {
                handler(
                    messageToPublisher,
                    EventStore.PublishersBySubscription,
                    NotificationsByCorrelations(connection),
                    PublisherVersionByPublisherDataContractCorrelations(connection),
                    () => DateTimeOffset.Now,
                    SaveNotificationsByPublisherAndVersion(connection),
                    messages => post(messages));
            });
        }

        private static void Handle(SubscriberMessage message, Post post)
        {
            var messageToPublisher = new MessageToPublisher
            {
                Notification = message.Notification,
                Subscription = message.Subscription
            };

            var notifyPublisher = NotifyPublisher(post);

            notifyPublisher(messageToPublisher, PublisherChannel.Handler, CommitEventStoreConnection);
        }
    }

    public static class EventStore
    {
        static EventStore()
        {
            PublishersBySubscription = new PublishersBySubscription();
        }

        public static PublishersBySubscription PublishersBySubscription { get; set; }
    }
}