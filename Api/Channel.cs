using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class Channel
    {
        public static IEnumerable<MessageToConsumer<TEndpoint>> PrepareMessages<TEndpoint>(
            IDomainEvent notification,
            ConsumersBySubscription<TEndpoint> consumersBySubscription)
        {
            return consumersBySubscription
                .Where(p => p.Key.NotificationContract.Equals(new TypeContract(notification)))
                .Select(p => new MessageToConsumer<TEndpoint> { Notification = notification, Subscription = p.Key });
        }

        public static void Push<TEndpoint>(
            MessageToConsumer<TEndpoint> messageToConsumer,
            ConsumersBySubscription<TEndpoint> consumersBySubscription,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<DateTimeOffset> clock,
            TEndpoint endpoint)
        {
            var consumer = consumersBySubscription[messageToConsumer.Subscription];

            consumer(messageToConsumer.Notification, notificationsByCorrelations, clock, endpoint);
        }

        public static IEnumerable<MessageToPublisher> PrepareMessages(
            IDomainEvent notification,
            PublishersBySubscription publishersBySubscription)
        {
            return publishersBySubscription
                .Where(p => p.Key.NotificationContract.Equals(new TypeContract(notification)))
                .Select(p => new MessageToPublisher {Notification = notification, Subscription = p.Key});
        }

        public static void Push(
            MessageToPublisher messageToPublisher, 
            PublishersBySubscription publishersBySubscription,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
            Func<DateTimeOffset> clock,
            Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
            Action<IEnumerable<MessageToPublisher>> notify)
        {
            var publisher = publishersBySubscription[messageToPublisher.Subscription];

            var notificationsByPublisher = publisher(messageToPublisher.Notification, notificationsByCorrelations, clock);

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

    public struct MessageToPublisher : Message
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }

    public struct MessageToConsumer<TEndpoint> : Message
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }

    public interface Message { }
}