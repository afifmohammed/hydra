using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public class Publisher<TData> : Given<TData>
        where TData : new()
    {
        public CorrelationMap<TData, TNotification> Given<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<TData, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(), 
                new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> { Type<TData>.Maps(mapper) },
                new PublishersBySubscription()
            );
        }
    }

    class NotificationHandler<TData, TNotification> : CorrelationMap<TData, TNotification>
        where TData : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _publisherDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> _publisherDataMappers;
        public PublishersBySubscription PublisherBySubscription { get; }

        public NotificationHandler(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps, 
            List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> mappers,
            PublishersBySubscription publisherByNotificationAndPublisherContract)
        {
            _publisherDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();
            _publisherDataMappers = mappers ?? new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>>();

            PublisherBySubscription = publisherByNotificationAndPublisherContract ?? new PublishersBySubscription();
        }

        public CorrelationMap<TData, TNotification1> Given<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, PublisherBySubscription);
        }

        public CorrelationMap<TData, TNotification1> When<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, PublisherBySubscription);
        }

        public CorrelationMap<TData, TNotification1> When<TNotification1>() where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public PublisherSubscriptions<TData> Then(Func<TData, TNotification, IEnumerable<IDomainEvent>> handler)
        {
            PublisherBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(TData).Contract()),
                (notification, queryNotificationsByCorrelations, clock) => Functions.BuildPublisher
                                    (
                                        handler,
                                        _publisherDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(a => a.Value)),
                                        queryNotificationsByCorrelations,
                                        Extensions.Correlations,
                                        _publisherDataMappers.ToDictionary(x => x.Key, x => x.Value),
                                        clock
                                    )((TNotification)notification)
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