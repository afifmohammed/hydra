using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class Channel
    {
        public static IEnumerable<Message> PrepareMessages(
            IDomainEvent notification,
            PublishersBySubscription publishersBySubscription)
        {
            return publishersBySubscription
                .Where(p => p.Key.NotificationContract.Equals(new TypeContract(notification)))
                .Select(p => new Message {Notification = notification, Subscription = p.Key});
        }

        public static void Push(
            Message message, 
            PublishersBySubscription publishersBySubscription,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
            Func<DateTimeOffset> clock,
            Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
            Action<IEnumerable<Message>> notify)
        {
            var publisher = publishersBySubscription[message.Subscription];

            var notificationsByPublisher = publisher(message.Notification, notificationsByCorrelations, clock);

            var notificationsByPublisherAndVersion = Functions.AppendPublisherVersion(
                notificationsByPublisher,
                publisherVersionByPublisherDataContractCorrelations);

            saveNotificationsByPublisherAndVersion(notificationsByPublisherAndVersion);

            notify(notificationsByPublisher
                .Notifications
                .SelectMany(n => PrepareMessages(n.Item1, publishersBySubscription))
                .ToArray());
        }
    }

    public struct Message
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }
}