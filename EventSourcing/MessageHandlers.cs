﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public class SubscriberMessage
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }

    public class MessageToPublisher : SubscriberMessage
    {}

    public class MessageToConsumer<TEndpoint> : SubscriberMessage
    {}

    public delegate IEnumerable<MessageToPublisher> PrepareMessages(
        IDomainEvent notification,
        PublishersBySubscription publishersBySubscription);

    public delegate IEnumerable<MessageToConsumer<TEndpoint>> PrepareMessages<TEndpoint>(
        IDomainEvent notification,
        ConsumersBySubscription<TEndpoint> consumersBySubscription);

    public delegate void Handler(
        MessageToPublisher messageToPublisher,
        PublishersBySubscription publishersBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
        Func<DateTimeOffset> clock,
        Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
        Action<IEnumerable<MessageToPublisher>> notify);

    public delegate void Handler<TEndpoint>(
        MessageToConsumer<TEndpoint> messageToConsumer,
        ConsumersBySubscription<TEndpoint> consumersBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint endpoint);

    public static class ConsumerChannel<TEndpoint>
    {
        public static PrepareMessages<TEndpoint> PrepareMessages = (notification, consumersBySubscription) => 
            consumersBySubscription
                .Where(p => p.Key.NotificationContract.Equals(new TypeContract(notification)))
                .Select(p => new MessageToConsumer<TEndpoint> { Notification = notification, Subscription = p.Key });

        public static Handler<TEndpoint> Handler => (
            messageToConsumer, 
            consumersBySubscription, 
            notificationsByCorrelations, 
            clock, 
            endpoint) => 
            consumersBySubscription[messageToConsumer.Subscription]
            (
                messageToConsumer.Notification, 
                notificationsByCorrelations, 
                clock, 
                endpoint
            );
    }

    public static class PublisherChannel
    {
        public static PrepareMessages PrepareMessages = (notification, publishersBySubscription) =>  
            publishersBySubscription
                .Where(p => p.Key.NotificationContract.Equals(new TypeContract(notification)))
                .Select(p => new MessageToPublisher {Notification = notification, Subscription = p.Key});

        public static Handler Handler = (
            messageToPublisher, 
            publishersBySubscription, 
            notificationsByCorrelations, 
            publisherVersionByPublisherDataContractCorrelations, 
            clock, 
            saveNotificationsByPublisherAndVersion, 
            notify) =>
            {
                var publisher = publishersBySubscription[messageToPublisher.Subscription];

                var notificationsByPublisher = publisher(messageToPublisher.Notification, notificationsByCorrelations,
                    clock);

                var notificationsByPublisherAndVersion = Functions.AppendPublisherVersion(
                    notificationsByPublisher,
                    publisherVersionByPublisherDataContractCorrelations);

                saveNotificationsByPublisherAndVersion(notificationsByPublisherAndVersion);

                notify(notificationsByPublisher
                    .Notifications
                    .SelectMany(n => PrepareMessages(n.Item1, publishersBySubscription))
                    .ToArray());
            };
    }
}