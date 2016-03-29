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
        CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, Action<TData>> mapper) where TNotification : IDomainEvent;
    }

    public interface When<TData> where TData : new()
    {
        CorrelationMap<TData, TNotification> When<TNotification>(Func<TNotification, Action<TData>> mapper) where TNotification : IDomainEvent;
    }

    public interface Then<TData, TNotification> : When<TData>
        where TData : new()
        where TNotification : IDomainEvent
    {
        Build<TData, TNotification> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler);
    }

    public interface Build<TData, in TNotification> : When<TData>
        where TData : new()
        where TNotification : IDomainEvent
    {
        IEnumerable<Func<TNotification, NotificationsByPublisher>> Build();
    }
}
