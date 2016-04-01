using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class Channel
    {
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
                .SelectMany(n => publishersBySubscription
                    .Where(p => p.Key.NotificationContract.Equals(new TypeContract(n)))
                    .Select(p => new Message { Notification = n.Item1, Subscription = p.Key }))
                .ToArray());
        }
    }

    public struct Message
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }
}