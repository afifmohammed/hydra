using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public class SideEffects<TData, TEndpoint> : 
        Given<TData, TEndpoint>,
        When<TData, TEndpoint> 
        where TData : new()
    {
        public CorrelationMap<TData, TNotification, TEndpoint> Given<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<TData, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> { Type<TData>.Maps(mapper) },
                new SubscribersBySubscription<TEndpoint>()
            );
        }

        public CorrelationMap<TData, TNotification, TEndpoint> When<TNotification>(Func<TNotification, TData, TData> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<TData, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> { Type<TData>.Maps(mapper) },
                new SubscribersBySubscription<TEndpoint>()
            );
        }

        public CorrelationMap<TData, TNotification, TEndpoint> When<TNotification>() where TNotification : IDomainEvent
        {
            return new NotificationHandler<TData, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>>(),
                new SubscribersBySubscription<TEndpoint>()
            );
        }
    }

    class NotificationHandler<TData, TNotification, TEndpoint> : CorrelationMap<TData, TNotification, TEndpoint>
        where TData : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _subscriberDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> _subscriberDataMappers;
        public SubscribersBySubscription<TEndpoint> SubscriberBySubscription { get; }

        public NotificationHandler(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>> mappers,
            SubscribersBySubscription<TEndpoint> subscriberByNotificationAndSubscriberContract)
        {
            _subscriberDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();
            _subscriberDataMappers = mappers ?? new List<KeyValuePair<TypeContract, Func<TData, JsonContent, TData>>>();

            SubscriberBySubscription = subscriberByNotificationAndSubscriberContract ?? new SubscribersBySubscription<TEndpoint>();
        }

        public CorrelationMap<TData, TNotification1, TEndpoint> Given<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1, TEndpoint>(_subscriberDataContractMaps, _subscriberDataMappers, SubscriberBySubscription);
        }

        public CorrelationMap<TData, TNotification1, TEndpoint> When<TNotification1>(Func<TNotification1, TData, TData> mapper) where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TData>.Maps(mapper));
            return new NotificationHandler<TData, TNotification1, TEndpoint>(_subscriberDataContractMaps, _subscriberDataMappers, SubscriberBySubscription);
        }

        public CorrelationMap<TData, TNotification1, TEndpoint> When<TNotification1>() where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public SubscriberSubscriptions<TData, TEndpoint> Then(Action<TData, TNotification, TEndpoint> handler)
        {
            SubscriberBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(TData).Contract()),
                (notification, queryNotificationsByCorrelations, clock, connection) => Functions.BuildSubscriber
                                    (
                                        handler,
                                        _subscriberDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(a => a.Value)),
                                        queryNotificationsByCorrelations,
                                        connection,
                                        _subscriberDataMappers.ToDictionary(x => x.Key, x => x.Value),
                                        clock
                                    )((TNotification)notification)
            );

            return this;
        }

        public CorrelationMap<TData, TNotification, TEndpoint> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<TData, object>> right)
        {
            _subscriberDataContractMaps.Add(Type<TData>.Correlates(right, left));
            return this;
        }
    }
}