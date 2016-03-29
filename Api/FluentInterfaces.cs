using System;
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

    public interface When<TData> where TData : new()
    {
        CorrelationMap<TData, TNotification> When<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent;
        CorrelationMap<TData, TNotification> When<TNotification>() where TNotification : IDomainEvent;
        IEnumerable<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>> Publishers { get; }
    }

    public interface Then<TData, out TNotification> : When<TData>
        where TData : new()
        where TNotification : IDomainEvent
    {
        When<TData> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler);
    }
}
