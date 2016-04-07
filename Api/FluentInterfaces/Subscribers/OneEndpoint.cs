using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public delegate void Subscriber<in TEndpoint>(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint connection);

    public class SubscribersBySubscription<TEndpoint> : Dictionary<Subscription, Subscriber<TEndpoint>>
    { }

    public interface SubscriberContractSubscriptions<THandlerContract, TEndpoint> :
        SubscriberSubscriptions<TEndpoint>,
        When<THandlerContract, TEndpoint>
        where THandlerContract : new()
    { }

    public interface SubscriberSubscriptions<TEndpoint>
    {
        SubscribersBySubscription<TEndpoint> SubscriberBySubscription { get; }
    }

    public interface CorrelationMap<THandlerContract, TNotification, TEndpoint> :
        Given<THandlerContract, TEndpoint>,
        SubscriberContractSubscriptions<THandlerContract, TEndpoint>,
        Then<THandlerContract, TNotification, TEndpoint>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        CorrelationMap<THandlerContract, TNotification, TEndpoint> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<THandlerContract, object>> right);
    }

    public interface Given<THandlerContract, TEndpoint>
        where THandlerContract : new()
    {
        CorrelationMap<THandlerContract, TNotification, TEndpoint> Given<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent;
    }

    public interface When<THandlerContract, TEndpoint>
        where THandlerContract : new()
    {
        CorrelationMap<THandlerContract, TNotification, TEndpoint> When<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent;
        CorrelationMap<THandlerContract, TNotification, TEndpoint> When<TNotification>() where TNotification : IDomainEvent;
    }

    public interface Then<THandlerContract, out TNotification, TEndpoint>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        SubscriberContractSubscriptions<THandlerContract, TEndpoint> Then(Action<THandlerContract, TNotification, TEndpoint> handler);
    }

    public class SubscriberBuilder<THandlerContract, TEndpoint> :
        Given<THandlerContract, TEndpoint>,
        When<THandlerContract, TEndpoint>
        where THandlerContract : new()
    {
        public CorrelationMap<THandlerContract, TNotification, TEndpoint> Given<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> { Type<THandlerContract>.Maps(mapper) },
                new SubscribersBySubscription<TEndpoint>()
            );
        }

        public CorrelationMap<THandlerContract, TNotification, TEndpoint> When<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> { Type<THandlerContract>.Maps(mapper) },
                new SubscribersBySubscription<TEndpoint>()
            );
        }

        public CorrelationMap<THandlerContract, TNotification, TEndpoint> When<TNotification>() where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>>(),
                new SubscribersBySubscription<TEndpoint>()
            );
        }
    }

    class NotificationHandler<THandlerContract, TNotification, TEndpoint> : CorrelationMap<THandlerContract, TNotification, TEndpoint>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _subscriberDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> _subscriberDataMappers;
        public SubscribersBySubscription<TEndpoint> SubscriberBySubscription { get; }

        public NotificationHandler(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> mappers,
            SubscribersBySubscription<TEndpoint> subscriberByNotificationAndSubscriberContract)
        {
            _subscriberDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();
            _subscriberDataMappers = mappers ?? new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>>();

            SubscriberBySubscription = subscriberByNotificationAndSubscriberContract ?? new SubscribersBySubscription<TEndpoint>();
        }

        public CorrelationMap<THandlerContract, TNotification1, TEndpoint> Given<TNotification1>(Func<TNotification1, THandlerContract, THandlerContract> mapper) where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<THandlerContract>.Maps(mapper));
            return new NotificationHandler<THandlerContract, TNotification1, TEndpoint>(_subscriberDataContractMaps, _subscriberDataMappers, SubscriberBySubscription);
        }

        public CorrelationMap<THandlerContract, TNotification1, TEndpoint> When<TNotification1>(Func<TNotification1, THandlerContract, THandlerContract> mapper) where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<THandlerContract>.Maps(mapper));
            return new NotificationHandler<THandlerContract, TNotification1, TEndpoint>(_subscriberDataContractMaps, _subscriberDataMappers, SubscriberBySubscription);
        }

        public CorrelationMap<THandlerContract, TNotification1, TEndpoint> When<TNotification1>() where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public SubscriberContractSubscriptions<THandlerContract, TEndpoint> Then(Action<THandlerContract, TNotification, TEndpoint> handler)
        {
            SubscriberBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(THandlerContract).Contract()),
                (notification, queryNotificationsByCorrelations, clock, endpoint) => Functions.BuildSubscriber
                                    (
                                        handler,
                                        _subscriberDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(a => a.Value)),
                                        queryNotificationsByCorrelations,
                                        endpoint,
                                        _subscriberDataMappers.ToDictionary(x => x.Key, x => x.Value),
                                        clock
                                    )((TNotification)notification)
            );

            return this;
        }

        public CorrelationMap<THandlerContract, TNotification, TEndpoint> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<THandlerContract, object>> right)
        {
            _subscriberDataContractMaps.Add(Type<THandlerContract>.Correlates(right, left));
            return this;
        }
    }
}