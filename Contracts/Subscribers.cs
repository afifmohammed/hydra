using System;
using System.Collections.Generic;

namespace Hydra.Core
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
        public Event Event { get; set; }
    }

    public delegate void Subscriber(SubscriberMessage message);

    public delegate NotificationsByPublisher Publisher(
        Event @event,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock);

    public delegate void Projector<in TProvider>(
        Event @event,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TProvider provider)
        where TProvider : IProvider;

    public delegate void Connecter<in TLeftProvider, in TRightProvider>(
        Event @event,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TLeftProvider leftProvider,
        TRightProvider rightProvider)
        where TLeftProvider : IProvider
        where TRightProvider : IProvider;

    public class PublishersBySubscription : Dictionary<Subscription, Publisher>, IDisposable
    {
        public void Dispose()
        {}
    }

    public class ProjectorsBySubscription<TProvider> : Dictionary<Subscription, Projector<TProvider>>, IDisposable
        where TProvider : IProvider
    {
        public void Dispose()
        {}
    }

    public class ConnectersBySubscription<TLeftProvider, TRightProvider> : Dictionary<Subscription, Connecter<TLeftProvider, TRightProvider>>, IDisposable
        where TLeftProvider : IProvider
        where TRightProvider : IProvider
    {
        public void Dispose()
        {}
    }
}