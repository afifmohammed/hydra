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

        public static Func<Post, NotifyPublisher<TPersistence>> NotifyPublisher = post => (messageToPublisher, handler, commitWork) =>
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

        public static CommitWork<TPersistence> CommitEventStoreConnection { get; set; }

        public static Func<Post, Handle> Submit =
            post =>
                message =>
                {
                    var messageToPublisher = new MessageToPublisher
                    {
                        Notification = message.Notification,
                        Subscription = message.Subscription
                    };

                    var notifyPublisher = NotifyPublisher(messages => post(messages));

                    notifyPublisher(messageToPublisher, PublisherChannel.Handler, CommitEventStoreConnection);
                };
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