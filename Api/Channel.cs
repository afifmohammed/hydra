﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class Channel
    {
        public static IEnumerable<SubscriberNotification<TEndpoint>> PrepareMessages<TEndpoint>(
            IDomainEvent notification,
            SubscribersBySubscription<TEndpoint> subscribersBySubscription)
        {
            return subscribersBySubscription
                .Where(p => p.Key.NotificationContract.Equals(new TypeContract(notification)))
                .Select(p => new SubscriberNotification<TEndpoint> { Notification = notification, Subscription = p.Key });
        }

        public static void Push<TEndpoint>(
            SubscriberNotification<TEndpoint> subscriberNotification,
            SubscribersBySubscription<TEndpoint> subscribersBySubscription,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<DateTimeOffset> clock,
            TEndpoint endpoint)
        {
            var subscriber = subscribersBySubscription[subscriberNotification.Subscription];

            subscriber(subscriberNotification.Notification, notificationsByCorrelations, clock, endpoint);
        }

        public static IEnumerable<PublisherNotification> PrepareMessages(
            IDomainEvent notification,
            PublishersBySubscription publishersBySubscription)
        {
            return publishersBySubscription
                .Where(p => p.Key.NotificationContract.Equals(new TypeContract(notification)))
                .Select(p => new PublisherNotification {Notification = notification, Subscription = p.Key});
        }

        public static void Push(
            PublisherNotification publisherNotification, 
            PublishersBySubscription publishersBySubscription,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<IEnumerable<Correlation>, int> publisherVersionByPublisherDataContractCorrelations,
            Func<DateTimeOffset> clock,
            Action<NotificationsByPublisherAndVersion> saveNotificationsByPublisherAndVersion,
            Action<IEnumerable<PublisherNotification>> notify)
        {
            var publisher = publishersBySubscription[publisherNotification.Subscription];

            var notificationsByPublisher = publisher(publisherNotification.Notification, notificationsByCorrelations, clock);

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

    public struct PublisherNotification : Message
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }

    public struct SubscriberNotification<TEndpoint> : Message
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }

    public interface Message { }
}