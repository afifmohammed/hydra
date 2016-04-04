﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EventSourcing
{
    public interface CorrelationMap<TData, TNotification> : Given<TData>, When<TData>, Then<TData, TNotification>
        where TData : new()
        where TNotification : IDomainEvent
    {
        CorrelationMap<TData, TNotification> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<TData, object>> right);
    }

    public interface Given<TData>
        where TData : new()
    {
        CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent;
    }

    public struct Subscription
    {
        public Subscription(TypeContract notificationContract, TypeContract publisherDataContract)
        {
            NotificationContract = notificationContract;
            PublisherDataContract = publisherDataContract;
        }

        public TypeContract NotificationContract { get; set; }
        public TypeContract PublisherDataContract { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if ((obj is Subscription) == false) return false;
            
            return Equals((Subscription)obj);
        }

        public bool Equals(Subscription other)
        {
            return NotificationContract.Equals(other.NotificationContract) && PublisherDataContract.Equals(other.PublisherDataContract);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NotificationContract.GetHashCode()*397) ^ PublisherDataContract.GetHashCode();
            }
        }
    }

    public delegate NotificationsByPublisher Publisher(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock);

    public class PublishersBySubscription : Dictionary<Subscription, Publisher>
    { }

    public interface PublisherSubscriptions
    {
        PublishersBySubscription PublisherBySubscription { get; }
    }

    public interface When<TData> : PublisherSubscriptions
        where TData : new()
    {
        CorrelationMap<TData, TNotification> When<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent;
        CorrelationMap<TData, TNotification> When<TNotification>() where TNotification : IDomainEvent;
    }

    public interface Then<TData, out TNotification> : When<TData>
        where TData : new()
        where TNotification : IDomainEvent
    {
        When<TData> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler);
    }
}
