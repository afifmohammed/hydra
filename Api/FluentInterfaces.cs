using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EventSourcing
{
    public interface CorrelationMap<TData, TNotification> : Given<TData>, When<TData>, Then<TData, TNotification>
        where TData : new()
    {
        CorrelationMap<TData, TNotification> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<TData, object>> right);
    }

    public interface Given<TData>
        where TData : new()
    {
        CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, Action<TData>> mapper);
    }

    public interface When<TData> where TData : new()
    {
        CorrelationMap<TData, TNotification> When<TNotification>(Func<TNotification, Action<TData>> mapper);
    }

    public interface Then<TData, out TNotification> : When<TData>
        where TData : new()
    {
        When<TData> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler);
    }
}
