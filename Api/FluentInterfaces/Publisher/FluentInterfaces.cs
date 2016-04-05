using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EventSourcing
{
    public interface CorrelationMap<TData, TNotification> : Given<TData>, PublisherSubscriptions<TData>, Then<TData, TNotification>
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
                return (NotificationContract.GetHashCode()*397) ^ SubscriberDataContract.GetHashCode();
            }
        }
    }

    public delegate NotificationsByPublisher Publisher(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock);

    public class PublishersBySubscription : Dictionary<Subscription, Publisher>
    { }

    public interface PublisherSubscriptions<TData> :
        When<TData>,
        PublisherSubscriptions
        where TData : new()
    {}

    public interface PublisherSubscriptions
    {
        PublishersBySubscription PublisherBySubscription { get; }
    }

    public interface When<TData>
        where TData : new()
    {
        CorrelationMap<TData, TNotification> When<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent;
        CorrelationMap<TData, TNotification> When<TNotification>() where TNotification : IDomainEvent;
    }

    public interface Then<TData, out TNotification>
        where TData : new()
        where TNotification : IDomainEvent
    {
        PublisherSubscriptions<TData> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler);
    }
}
