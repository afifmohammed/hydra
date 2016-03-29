using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public class UseCase<TData> : Given<TData>
        where TData : new()
    {
        public CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, Action<TData>> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<TData, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(), 
                new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> { Type<TData>.Maps(mapper) }
            );
        }
    }

    class NotificationHandler<TData, TNotification> : CorrelationMap<TData, TNotification>, Build<TData, TNotification>
        where TData : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _publisherDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> _publisherDataMappers;
        readonly List<Func<TData, TNotification, IEnumerable<IDomainEvent>>> _handlers = new List<Func<TData, TNotification, IEnumerable<IDomainEvent>>>();

        public NotificationHandler(List<KeyValuePair<TypeContract, CorrelationMap>> maps, List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> mappers)
        {
            _publisherDataContractMaps = maps;
            _publisherDataMappers = mappers;
        }
        public CorrelationMap<TData, TNotification1> Given<TNotification1>(Func<TNotification1, Action<TData>> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers);
        }

        public CorrelationMap<TData, TNotification1> When<TNotification1>(Func<TNotification1, Action<TData>> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers);
        }

        public Build<TData, TNotification> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler)
        {
            _handlers.Add(handler);
            return this;
        }

        public CorrelationMap<TData, TNotification> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<TData, object>> right)
        {
            _publisherDataContractMaps.Add(Type<TData>.Correlates(right, left));
            return this;
        }

        public IEnumerable<Func<TNotification, NotificationsByPublisher>> Build()
        {
            return _handlers.Select
            (
                handler => 
                    Functions.GroupNotificationsByPublisher
                    (
                        handler,
                        _publisherDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(a => a.Value)),
                        GlobalConfiguration.NotificationsByCorrelations,
                        Extensions.Correlations,
                        _publisherDataMappers.ToDictionary(x => x.Key, x => x.Value),
                        GlobalConfiguration.Clock
                    )
            );
        }
    }
}