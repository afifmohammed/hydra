using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public delegate Func<IEnumerable<Correlation>, int> PublisherVersionByCorrelationsFunction<in TProvider>(
        TProvider provider) 
        where TProvider : IProvider;

    public delegate IEnumerable<SerializedNotification> NotificationsByCorrelations(
        IEnumerable<Correlation> correlation);

    public delegate NotificationsByCorrelations NotificationsByCorrelationsFunction<in TProvider>(
        TProvider provider) 
        where TProvider : IProvider;

    public delegate Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersionAction<in TProvider>(
        TProvider provider) 
        where TProvider : IProvider;

    public static class EventStore<TProvider> where TProvider : IProvider
    {
        public static NotificationsByCorrelationsFunction<TProvider> NotificationsByCorrelationsFunction { get; set; }
        public static PublisherVersionByCorrelationsFunction<TProvider> PublisherVersionByCorrelationsFunction { get; set; }
        public static SaveNotificationsByPublisherAndVersionAction<TProvider> SaveNotificationsByPublisherAndVersionAction { get; set; }
        public static CommitWork<TProvider> CommitEventStoreWork { get; set; }

        public static Notify Notify = getSubscriptions => messages => { };

        public static Func<PublishersBySubscription, Func<IEnumerable<Subscription>>, Subscriber> Subscriber =
            (publishersBySubscription, getSubscriptions) =>
                message => 
                    HandleAndCommitAndPost
                    (
                        getSubscriptions,
                        message, 
                        publishersBySubscription,
                        NotificationsByCorrelationsFunction,
                        PublisherVersionByCorrelationsFunction,
                        SaveNotificationsByPublisherAndVersionAction,
                        CommitEventStoreWork, 
                        Notify
                    );

        internal static void HandleAndCommitAndPost(
            Func<IEnumerable<Subscription>> getSubscriptions,
            SubscriberMessage message,
            PublishersBySubscription publishersBySubscription,
            NotificationsByCorrelationsFunction<TProvider> notificationsByCorrelationsFunction,
            PublisherVersionByCorrelationsFunction<TProvider> publisherVersionByCorrelationsFunction,
            SaveNotificationsByPublisherAndVersionAction<TProvider> saveNotificationsByPublisherAndVersionAction,
            CommitWork<TProvider> commitWork, 
            Notify notify)
        {
            var list = new List<IDomainEvent>();

            commitWork(provider =>
            {
                Handle(
                    message,
                    publishersBySubscription,
                    notificationsByCorrelationsFunction(provider),
                    publisherVersionByCorrelationsFunction(provider),
                    () => DateTimeOffset.Now,
                    saveNotificationsByPublisherAndVersionAction(provider),
                    messages => list.AddRange(messages));
            });

            notify(getSubscriptions)(list);
        }

        internal static void Handle(
            SubscriberMessage message,
            PublishersBySubscription publishersBySubscription,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
            Func<DateTimeOffset> clock,
            Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
            Action<IEnumerable<IDomainEvent>> notify)
        {
            var publisher = publishersBySubscription[message.Subscription];

            var notificationsByPublisher = publisher(
                message.Notification,
                notificationsByCorrelations,
                clock);

            var notificationsByPublisherAndVersion = Functions.AppendPublisherVersion(
                notificationsByPublisher,
                publisherVersionByPublisherDataContractCorrelations);

            saveNotificationsByPublisherAndVersion(notificationsByPublisherAndVersion);

            notify(notificationsByPublisher.Notifications.Select(x => x.Item1));
        }
    }
}