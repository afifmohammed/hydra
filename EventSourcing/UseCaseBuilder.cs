using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EventSourcing
{
    interface Correlation<TData, TDecision, TTrigger> : When<TData>, Then<TData, TTrigger>
        where TData : new()
    {
        Correlation<TData, TDecision, TTrigger> Correlation(Expression<Func<TDecision, object>> id);
    }

    interface CorrelationMap<TData, TNotification> : Given<TData>, When<TData>, Then<TData, TNotification>
        where TData : new()
    {
        CorrelationMap<TData, TNotification> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<TData, object>> right);
    }

    interface When<TData> where TData : new()
    {
        CorrelationMap<TData, TNotification> When<TNotification>(Func<TNotification, Action<TData>> mapper);
    }

    interface Then<TData, TNotification> : When<TData>
        where TData : new()
    {
        Correlation<TData, TDecision, TNotification> Then<TDecision>(Func<TData, TNotification, TDecision> handler);
    }

    interface Given<TData>
        where TData : new()
    {
        CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, Action<TData>> mapper);
    }

    class UseCase<TData> : Given<TData>
        where TData : new()
    {
        public CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, Action<TData>> mapper)
        {
            throw new NotImplementedException();
        }

        public Then<TData, TNotification> When<TNotification>(Func<TNotification, Action<TData>> mapper)
        {
            throw new NotImplementedException();
        }
    }
}
