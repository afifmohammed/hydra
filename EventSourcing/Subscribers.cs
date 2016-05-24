﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public struct Subscription
    {
        public Subscription(TypeContract notificationContract, TypeContract subscriberDataContract)
        {
            NotificationContract = notificationContract;
            SubscriberDataContract = subscriberDataContract;
        }

        public TypeContract NotificationContract { get; set; }
        public TypeContract SubscriberDataContract { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if ((obj is Subscription) == false) return false;

            return Equals((Subscription)obj);
        }

        public bool Equals(Subscription other)
        {
            return NotificationContract.Equals(other.NotificationContract) && SubscriberDataContract.Equals(other.SubscriberDataContract);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NotificationContract.GetHashCode() * 397) ^ SubscriberDataContract.GetHashCode();
            }
        }
    }

    public class SubscriberMessage
    {
        public Subscription Subscription { get; set; }
        public IDomainEvent Notification { get; set; }
    }

    public static class SubscriberMessages
    {
        public static Func<IDomainEvent, IEnumerable<Subscription>, IEnumerable<SubscriberMessage>> By =
            (notification, subscriptions) =>
                subscriptions
                    .Where(subscription => subscription.NotificationContract.Equals(new TypeContract(notification)))
                    .Select(subscription => new SubscriberMessage { Notification = notification, Subscription = subscription });
    }

    public delegate void Subscriber(SubscriberMessage message);

    public delegate NotificationsByPublisher Publisher(
        IDomainEvent notification,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock);

    public delegate void Exporter<in TProvider>(
        IDomainEvent notification,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TProvider provider);

    public delegate void Integrator<in TLeftProvider, in TRightProvider>(
        IDomainEvent notification,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TLeftProvider leftProvider,
        TRightProvider rightProvider);

    public class PublishersBySubscription : Dictionary<Subscription, Publisher>
    { }

    public class ExportersBySubscription<TProvider> : Dictionary<Subscription, Exporter<TProvider>>
    { }

    public class IntegratorsBySubscription<TLeftProvider, TRightProvider> : Dictionary<Subscription, Integrator<TLeftProvider, TRightProvider>>
    { }
}