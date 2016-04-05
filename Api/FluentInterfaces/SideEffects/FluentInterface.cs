using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EventSourcing
{
    public delegate void Subscriber<in TEndpoint>(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint connection);

    public class SubscribersBySubscription<TEndpoint> : Dictionary<Subscription, Subscriber<TEndpoint>>
    { }

    public interface SubscriberSubscriptions<TData, TEndpoint> :
        SubscriberSubscriptions<TEndpoint>,
        When<TData, TEndpoint> 
        where TData : new()
    {}

    public interface SubscriberSubscriptions<TEndpoint>
    {
        SubscribersBySubscription<TEndpoint> SubscriberBySubscription { get; }
    }

    public interface CorrelationMap<TData, TNotification, TEndpoint> : 
        Given<TData, TEndpoint>,
        SubscriberSubscriptions<TData, TEndpoint>,
        Then<TData, TNotification, TEndpoint>
        where TData : new()
        where TNotification : IDomainEvent
    {
        CorrelationMap<TData, TNotification, TEndpoint> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<TData, object>> right);
    }

    public interface Given<TData, TEndpoint>
        where TData : new()
    {
        CorrelationMap<TData, TNotification, TEndpoint> Given<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent;
    }

    public interface When<TData, TEndpoint>
        where TData : new()
    {
        CorrelationMap<TData, TNotification, TEndpoint> When<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent;
        CorrelationMap<TData, TNotification, TEndpoint> When<TNotification>() where TNotification : IDomainEvent;
    }

    public interface Then<TData, out TNotification, TEndpoint>
        where TData : new()
        where TNotification : IDomainEvent
    {
        SubscriberSubscriptions<TData, TEndpoint> Then(Action<TData, TNotification, TEndpoint> handler);
    }
}