using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public delegate void Subscriber<in TEndpoint1, in TEndpoint2>(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint1 endpoint1, 
        TEndpoint2 endpoint2);

    public class SubscribersBySubscription<TEndpoint1, TEndpoint2> : Dictionary<Subscription, Subscriber<TEndpoint1, TEndpoint2>>
    { }

    public interface SubscriberContractSubscriptions<THandlerContract, TEndpoint1, TEndpoint2> :
        SubscriberSubscriptions<TEndpoint1, TEndpoint2>,
        When<THandlerContract, TEndpoint1, TEndpoint2>
        where THandlerContract : new()
    { }

    public interface SubscriberSubscriptions<TEndpoint1, TEndpoint2>
    {
        SubscribersBySubscription<TEndpoint1, TEndpoint2> SubscriberBySubscription { get; }
    }

    public interface CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> :
        Given<THandlerContract, TEndpoint1, TEndpoint2>,
        SubscriberContractSubscriptions<THandlerContract, TEndpoint1, TEndpoint2>,
        Then<THandlerContract, TNotification, TEndpoint1, TEndpoint2>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<THandlerContract, object>> right);
    }

    public interface Given<THandlerContract, TEndpoint1, TEndpoint2>
        where THandlerContract : new()
    {
        CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> Given<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent;
    }

    public interface When<THandlerContract, TEndpoint1, TEndpoint2>
        where THandlerContract : new()
    {
        CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent;
        CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>() where TNotification : IDomainEvent;
    }

    public interface Then<THandlerContract, out TNotification, TEndpoint1, TEndpoint2>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        SubscriberContractSubscriptions<THandlerContract, TEndpoint1, TEndpoint2> Then(Action<THandlerContract, TNotification, TEndpoint1, TEndpoint2> handler);
    }

    public class SubscriberBuilder<THandlerContract, TEndpoint1, TEndpoint2> :
        Given<THandlerContract, TEndpoint1, TEndpoint2>,
        When<THandlerContract, TEndpoint1, TEndpoint2>
        where THandlerContract : new()
    {
        public CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> Given<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification, TEndpoint1, TEndpoint2>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> { Type<THandlerContract>.Maps(mapper) },
                new SubscribersBySubscription<TEndpoint1, TEndpoint2>()
            );
        }

        public CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification, TEndpoint1, TEndpoint2>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> { Type<THandlerContract>.Maps(mapper) },
                new SubscribersBySubscription<TEndpoint1, TEndpoint2>()
            );
        }

        public CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>() where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification, TEndpoint1, TEndpoint2>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>>(),
                new SubscribersBySubscription<TEndpoint1, TEndpoint2>()
            );
        }
    }

    class NotificationHandler<THandlerContract, TNotification, TEndpoint1, TEndpoint2> : CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _subscriberDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> _subscriberDataMappers;
        public SubscribersBySubscription<TEndpoint1, TEndpoint2> SubscriberBySubscription { get; }

        public NotificationHandler(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> mappers,
            SubscribersBySubscription<TEndpoint1, TEndpoint2> subscriberByNotificationAndSubscriberContract)
        {
            _subscriberDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();
            _subscriberDataMappers = mappers ?? new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>>();

            SubscriberBySubscription = subscriberByNotificationAndSubscriberContract ?? new SubscribersBySubscription<TEndpoint1, TEndpoint2>();
        }

        public CorrelationMap<THandlerContract, TNotification1, TEndpoint1, TEndpoint2> Given<TNotification1>(Func<TNotification1, THandlerContract, THandlerContract> mapper) where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<THandlerContract>.Maps(mapper));
            return new NotificationHandler<THandlerContract, TNotification1, TEndpoint1, TEndpoint2>(_subscriberDataContractMaps, _subscriberDataMappers, SubscriberBySubscription);
        }

        public CorrelationMap<THandlerContract, TNotification1, TEndpoint1, TEndpoint2> When<TNotification1>(Func<TNotification1, THandlerContract, THandlerContract> mapper) where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<THandlerContract>.Maps(mapper));
            return new NotificationHandler<THandlerContract, TNotification1, TEndpoint1, TEndpoint2>(_subscriberDataContractMaps, _subscriberDataMappers, SubscriberBySubscription);
        }

        public CorrelationMap<THandlerContract, TNotification1, TEndpoint1, TEndpoint2> When<TNotification1>() where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public SubscriberContractSubscriptions<THandlerContract, TEndpoint1, TEndpoint2> Then(Action<THandlerContract, TNotification, TEndpoint1, TEndpoint2> handler)
        {
            SubscriberBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(THandlerContract).Contract()),
                (notification, queryNotificationsByCorrelations, clock, endpoint1, endpoint2) => Functions.BuildSubscriber
                                    (
                                        handler,
                                        _subscriberDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(a => a.Value)),
                                        queryNotificationsByCorrelations,
                                        endpoint1,
                                        endpoint2,
                                        _subscriberDataMappers.ToDictionary(x => x.Key, x => x.Value),
                                        clock
                                    )((TNotification)notification)
            );

            return this;
        }

        public CorrelationMap<THandlerContract, TNotification, TEndpoint1, TEndpoint2> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<THandlerContract, object>> right)
        {
            _subscriberDataContractMaps.Add(Type<THandlerContract>.Correlates(right, left));
            return this;
        }
    }
}