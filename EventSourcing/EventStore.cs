using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;

namespace Hydra.Subscribers
{
    public static class EventStore<TUowProvider> where TUowProvider : IUowProvider
    {
        public static NotificationsByCorrelationsFunction<TUowProvider> NotificationsByCorrelationsFunction { get; set; }
        public static PublisherVersionByCorrelationsFunction<TUowProvider> PublisherVersionByCorrelationsFunction { get; set; }
        public static SaveNotificationsByPublisherAndVersionAction<TUowProvider> SaveNotificationsByPublisherAndVersionAction { get; set; }
        public static CommitWork<TUowProvider> CommitEventStoreWork { get; set; }

        public static Action<IEnumerable<IDomainEvent>> Publish = events => { };

        public static Func<PublishersBySubscription, Subscriber> Subscriber =
            publishersBySubscription =>
                message => 
                    HandleAndCommitAndPost
                    (
                        message, 
                        publishersBySubscription,
                        NotificationsByCorrelationsFunction,
                        PublisherVersionByCorrelationsFunction,
                        SaveNotificationsByPublisherAndVersionAction,
                        CommitEventStoreWork, 
                        Publish
                    );

        internal static void HandleAndCommitAndPost(
            SubscriberMessage message,
            PublishersBySubscription publishersBySubscription,
            NotificationsByCorrelationsFunction<TUowProvider> notificationsByCorrelationsFunction,
            PublisherVersionByCorrelationsFunction<TUowProvider> publisherVersionByCorrelationsFunction,
            SaveNotificationsByPublisherAndVersionAction<TUowProvider> saveNotificationsByPublisherAndVersionAction,
            CommitWork<TUowProvider> commitWork, 
            Action<IEnumerable<IDomainEvent>> publish)
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

            publish(list);
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
                message.Event,
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