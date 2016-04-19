using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public delegate void NotifyPublisher<in TEndpoint>(MessageToPublisher messageToPublisher, CommitWork<TEndpoint> commitWork) where TEndpoint : class;
    public delegate Func<IEnumerable<Correlation>, int> PublisherVersionByPublisherDataContractCorrelations<in TEndpoint>(TEndpoint connection) where TEndpoint : class;
    public delegate IEnumerable<SerializedNotification> NotificationsByCorrelations(IEnumerable<Correlation> correlation);
    public delegate NotificationsByCorrelations NotificationsByCorrelations<in TEndpoint>(TEndpoint connection) where TEndpoint : class;
    public delegate Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersion<in TEndpoint>(TEndpoint connection) where TEndpoint : class;

    public static class EventStore<TPersistence> where TPersistence : class
    {
        public static NotificationsByCorrelations<TPersistence> NotificationsByCorrelations { get; set; }
        public static PublisherVersionByPublisherDataContractCorrelations<TPersistence> PublisherVersionByPublisherDataContractCorrelations { get; set; }
        public static SaveNotificationsByPublisherAndVersion<TPersistence> SaveNotificationsByPublisherAndVersion { get; set; }
    }

    public static class EventStore<TPersistence, TTransport>
        where TPersistence : class
        where TTransport : class
    {
        public static NotifyPublisher<TPersistence> NotifyPublisher = (messageToPublisher, commitWork) =>
            commitWork(connection =>
            {
                PublisherChannel.Push(
                    messageToPublisher,
                    EventStore.PublishersBySubscription,
                    EventStore<TPersistence>.NotificationsByCorrelations(connection),
                    EventStore<TPersistence>.PublisherVersionByPublisherDataContractCorrelations(connection),
                    () => DateTimeOffset.Now,
                    EventStore<TPersistence>.SaveNotificationsByPublisherAndVersion(connection),
                    publisherNotifications => Mailbox<TPersistence, TTransport>.Post(publisherNotifications.Cast<SubscriberMessage>()));
            });
    }

    public static class EventStore
    {
        public static PublishersBySubscription PublishersBySubscription { get; set; }
    }
}