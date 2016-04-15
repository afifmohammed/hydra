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

    public static class EventStore<TEndpoint> where TEndpoint : class
    {
        public static NotificationsByCorrelations<TEndpoint> NotificationsByCorrelations { get; set; }
        public static PublisherVersionByPublisherDataContractCorrelations<TEndpoint> PublisherVersionByPublisherDataContractCorrelations { get; set; }
        public static SaveNotificationsByPublisherAndVersion<TEndpoint> SaveNotificationsByPublisherAndVersion { get; set; }
    }

    public static class EventStore<TEndpoint, TTransport>
        where TEndpoint : class
        where TTransport : class
    {
        public static NotifyPublisher<TEndpoint> NotifyPublisher = (messageToPublisher, commitWork) =>
            commitWork(connection =>
            {
                Channel.Push(
                    messageToPublisher,
                    EventStore.PublishersBySubscription,
                    EventStore<TEndpoint>.NotificationsByCorrelations(connection),
                    EventStore<TEndpoint>.PublisherVersionByPublisherDataContractCorrelations(connection),
                    () => DateTimeOffset.Now,
                    EventStore<TEndpoint>.SaveNotificationsByPublisherAndVersion(connection),
                    publisherNotifications => Mailbox<TEndpoint, TTransport>.Post(publisherNotifications.Cast<SubscriberMessage>()));
            });
    }

    public static class EventStore
    {
        public static PublishersBySubscription PublishersBySubscription { get; set; }
    }
}