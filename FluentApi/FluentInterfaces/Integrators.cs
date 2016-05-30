using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hydra.Subscribers;

namespace Hydra.Core.FluentInterfaces
{
    public interface ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint1, TEndpoint2> :
        ConsumerSubscriptions<TEndpoint1, TEndpoint2>,
        When<TSubscriberDataContract, TEndpoint1, TEndpoint2>
        where TSubscriberDataContract : new()
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
    { }

    public interface ConsumerSubscriptions<TEndpoint1, TEndpoint2>
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
    {
        ConnectersBySubscription<TEndpoint1, TEndpoint2> ConnecterBySubscription { get; }
    }

    public interface CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> :
        Given<TSubscriberDataContract, TEndpoint1, TEndpoint2>,
        ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint1, TEndpoint2>,
        Then<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right);
    }

    public interface Given<TSubscriberDataContract, TEndpoint1, TEndpoint2>
        where TSubscriberDataContract : new()
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;
    }

    public interface When<TSubscriberDataContract, TEndpoint1, TEndpoint2>
        where TSubscriberDataContract : new()
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;

        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>() 
            where TNotification : IDomainEvent;
    }

    public interface Then<TSubscriberDataContract, out TNotification, TEndpoint1, TEndpoint2>
        where TSubscriberDataContract : new()
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
        where TNotification : IDomainEvent
    {
        ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint1, TEndpoint2> Then(
            Action<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> handler);
    }

    public class ConsumerBuilder<TSubscriberDataContract, TEndpoint1, TEndpoint2> :
        Given<TSubscriberDataContract, TEndpoint1, TEndpoint2>,
        When<TSubscriberDataContract, TEndpoint1, TEndpoint2>
        where TSubscriberDataContract : new()
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
    {
        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ConnectersBySubscription<TEndpoint1, TEndpoint2>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ConnectersBySubscription<TEndpoint1, TEndpoint2>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> When<TNotification>() 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>(),
                new ConnectersBySubscription<TEndpoint1, TEndpoint2>()
            );
        }
    }

    class ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> : 
        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TEndpoint1 : IUowProvider
        where TEndpoint2 : IUowProvider
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _subscriberDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> _subscriberDataMappers;
        public ConnectersBySubscription<TEndpoint1, TEndpoint2> ConnecterBySubscription { get; }

        public ConsumerCorrelationMap(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> mappers,
            ConnectersBySubscription<TEndpoint1, TEndpoint2> connecterByNotificationAndContract)
        {
            _subscriberDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();

            _subscriberDataMappers = mappers 
                ?? new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>();

            ConnecterBySubscription = connecterByNotificationAndContract ?? new ConnectersBySubscription<TEndpoint1, TEndpoint2>();
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint1, TEndpoint2> Given<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint1, TEndpoint2>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ConnecterBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint1, TEndpoint2> When<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint1, TEndpoint2>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ConnecterBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint1, TEndpoint2> When<TNotification1>() 
            where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint1, TEndpoint2> Then(
            Action<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> handler)
        {
            ConnecterBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(TSubscriberDataContract).Contract()),
                (@event, queryNotificationsByCorrelations, clock, endpoint1, endpoint2) => 
                    Functions.BuildIntegrator
                    (
                        handler,
                        _subscriberDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => (IReadOnlyCollection<CorrelationMap>)x.Select(a => a.Value).ToList()),
                        queryNotificationsByCorrelations,
                        endpoint1,
                        endpoint2,
                        _subscriberDataMappers.ToDictionary(x => x.Key, x => x.Value),
                        clock
                    )((TNotification)@event.Notification)
            );

            return this;
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint1, TEndpoint2> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right)
        {
            _subscriberDataContractMaps.Add(Type<TSubscriberDataContract>.Correlates(right, left));
            return this;
        }
    }
}