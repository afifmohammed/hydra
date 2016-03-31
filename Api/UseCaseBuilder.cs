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
                new Dictionary<
                    Tuple<TypeContract, TypeContract>,
                    Func<
                        IDomainEvent,
                        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>>,
                        Func<DateTimeOffset>,
                        NotificationsByPublisher>>()
            );
        }
    }

    class NotificationHandler<TData, TNotification> : CorrelationMap<TData, TNotification>
        where TData : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _publisherDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> _publisherDataMappers;
        public IDictionary<
            Tuple<TypeContract, TypeContract>,
            Func<
                IDomainEvent,
                Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>>,
                Func<DateTimeOffset>,
                NotificationsByPublisher>> PublisherByNotificationAndPublisherContract { get; }

        public NotificationHandler(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps, 
            List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> mappers,
            IDictionary<
                Tuple<TypeContract, TypeContract>,
                Func<
                    IDomainEvent,
                    Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>>,
                    Func<DateTimeOffset>,
                    NotificationsByPublisher>> publisherByNotificationAndPublisherContract)
        {
            _publisherDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();
            _publisherDataMappers = mappers ?? new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>>();

            PublisherByNotificationAndPublisherContract = publisherByNotificationAndPublisherContract 
                ?? new Dictionary<
                    Tuple<TypeContract, TypeContract>, 
                    Func<
                        IDomainEvent, 
                        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>>, 
                        Func<DateTimeOffset>, 
                        NotificationsByPublisher>>();
        }

        public CorrelationMap<TData, TNotification1> Given<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, PublisherByNotificationAndPublisherContract);
        }

        public CorrelationMap<TData, TNotification1> When<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, PublisherByNotificationAndPublisherContract);
        }

        public CorrelationMap<TData, TNotification1> When<TNotification1>() where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public When<TData> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler)
        {
            PublisherByNotificationAndPublisherContract.Add
            (
                new KeyValuePair<
                    Tuple<TypeContract, TypeContract>, 
                    Func<
                        IDomainEvent, 
                        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>>, 
                        Func<DateTimeOffset>, 
                        NotificationsByPublisher>>
                (
                    new Tuple<TypeContract, TypeContract>(typeof(TNotification).Contract(), typeof(TData).Contract()),
                    (e, query, clock) => Functions.GroupNotificationsByPublisher
                    (
                        handler,
                        _publisherDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(a => a.Value)),
                        query,
                        Extensions.Correlations,
                        _publisherDataMappers.ToDictionary(x => x.Key, x => x.Value),
                        clock
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
    }
}