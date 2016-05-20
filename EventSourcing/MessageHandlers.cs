using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public delegate void Handle(SubscriberMessage message);
    
    public class SubscriberMessage
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }

    public delegate void Handler(
        SubscriberMessage messageToPublisher,
        PublishersBySubscription publishersBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
        Func<DateTimeOffset> clock,
        Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
        Action<IEnumerable<SubscriberMessage>> notify);

    public delegate void Handler<TEndpoint>(
        SubscriberMessage messageToConsumer,
        ConsumersBySubscription<TEndpoint> consumersBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint endpoint);

    public delegate IEnumerable<SubscriberMessage> PrepareMessages(
        IDomainEvent notification,
        IEnumerable<Subscription> subscriptions);

    public static class Messages
    {
        public static PrepareMessages PrepareMessages = (notification, subscriptions) =>
                subscriptions
                    .Where(subscription => subscription.NotificationContract.Equals(new TypeContract(notification)))
                    .Select(subscription => new SubscriberMessage { Notification = notification, Subscription = subscription });
    }

    public static class HandlerWithNoSideEffects
    {
        public static Handler Handle = (
            SubscriberMessage message,
            PublishersBySubscription publishersBySubscription,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
            Func<DateTimeOffset> clock,
            Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
            Action<IEnumerable<SubscriberMessage>> notify) =>
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
                .SelectMany(n => Messages.PrepareMessages(n.Item1, publishersBySubscription.Keys))
                .ToArray());
        };
    }

    public static class HandlerWithSideEffectsTo<TEndpoint>
    {
        public static Handler<TEndpoint> Handle = (
            SubscriberMessage message,
            ConsumersBySubscription<TEndpoint> consumersBySubscription,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<DateTimeOffset> clock,
            TEndpoint endpoint) =>
        {
            var consumer = consumersBySubscription[message.Subscription];

            consumer
            (
                message.Notification,
                notificationsByCorrelations,
                clock,
                endpoint
            );
        };
    }
}