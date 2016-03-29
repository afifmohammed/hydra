using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public class UseCase<TData> : Given<TData>
        where TData : new()
    {
        public CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<TData, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(), 
                new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> { Type<TData>.Maps(mapper) },
                new List<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>>()
            );
        }
    }

    class NotificationHandler<TData, TNotification> : CorrelationMap<TData, TNotification>
        where TData : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _publisherDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> _publisherDataMappers;

        private readonly List<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>> _handlers;

        public NotificationHandler(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps, 
            List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> mappers,
            List<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>> handlers)
        {
            _publisherDataContractMaps = maps;
            _publisherDataMappers = mappers;
            _handlers = handlers;
        }
        public CorrelationMap<TData, TNotification1> Given<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, _handlers);
        }

        public CorrelationMap<TData, TNotification1> When<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, _handlers);
        }

        public CorrelationMap<TData, TNotification1> When<TNotification1>() where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public When<TData> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler)
        {
            _handlers.Add
            (
                new KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>
                (
                    new TypeContract(typeof(TNotification)),
                    e => Functions.GroupNotificationsByPublisher
                    (
                        handler,
                        _publisherDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(a => a.Value)),
                        GlobalConfiguration.NotificationsByCorrelations,
                        Extensions.Correlations,
                        _publisherDataMappers.ToDictionary(x => x.Key, x => x.Value),
                        GlobalConfiguration.Clock
                    )((TNotification)e)
                )
            );

            return this;
        }

        public CorrelationMap<TData, TNotification> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<TData, object>> right)
        {
            _publisherDataContractMaps.Add(Type<TData>.Correlates(right, left));
            return this;
        }

        public IEnumerable<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>> Publishers => _handlers;
    }
}