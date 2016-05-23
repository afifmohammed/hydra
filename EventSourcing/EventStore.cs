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
        public static Post Post = messages => { };

        public static Subscriber Subscriber = message => 
            HandleAndCommitAndPost
            (
                message, 
                EventStore.PublishersBySubscription,
                NotificationsByCorrelationsFunction,
                PublisherVersionByCorrelationsFunction,
                SaveNotificationsByPublisherAndVersionAction,
                CommitEventStoreWork, 
                Post
            );

        internal static void HandleAndCommitAndPost(
            SubscriberMessage message,
            PublishersBySubscription publishersBySubscription,
            NotificationsByCorrelationsFunction<TProvider> notificationsByCorrelationsFunction,
            PublisherVersionByCorrelationsFunction<TProvider> publisherVersionByCorrelationsFunction,
            SaveNotificationsByPublisherAndVersionAction<TProvider> saveNotificationsByPublisherAndVersionAction,
            CommitWork<TProvider> commitWork, 
            Post post)
        {
            var list = new List<SubscriberMessage>();

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

            post(list);
        }

        internal static void Handle(
            SubscriberMessage message,
            PublishersBySubscription publishersBySubscription,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
            Func<DateTimeOffset> clock,
            Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
            Action<IEnumerable<SubscriberMessage>> notify)
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

            notify(notificationsByPublisher
                .Notifications
                .SelectMany(n => SubscriberMessages.By(n.Item1, publishersBySubscription.Keys))
                .ToArray());
        }
    }

    public static class EventStore
    {
        static EventStore()
        {
            PublishersBySubscription = new PublishersBySubscription();
        }

        public static PublishersBySubscription PublishersBySubscription { get; set; }

        public static void Register(params PublishersBySubscription[][] subscriptions)
        {
            PublishersBySubscription = PublishersBySubscription ?? new PublishersBySubscription();

            foreach (var kvp in subscriptions
                .SelectMany(x => x)
                .SelectMany(subscription => subscription))
            {
                PublishersBySubscription.Add(kvp.Key, kvp.Value);
            }
        }
    }


}