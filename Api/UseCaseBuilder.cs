using System;

namespace EventSourcing
{
    public class UseCase<TData> : Given<TData>
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