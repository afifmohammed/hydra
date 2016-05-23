using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public interface ConsumerContractSubscriptions<TSubscriberContract, TEndpoint> :
        ConsumerSubscriptions<TEndpoint>,
        When<TSubscriberContract, TEndpoint>
        where TSubscriberContract : new()
    { }

    public interface ConsumerSubscriptions<TEndpoint>
    {
        ExportersBySubscription<TEndpoint> ExportersBySubscription { get; }
    }

    public interface CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> :
        Given<TSubscriberDataContract, TEndpoint>,
        ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint>,
        Then<TSubscriberDataContract, TNotification, TEndpoint>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right);
    }

    public interface Given<TSubscriberDataContract, TEndpoint>
        where TSubscriberDataContract : new()
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;
    }

    public interface When<TSubscriberDataContract, TEndpoint>
        where TSubscriberDataContract : new()
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;

        CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> When<TNotification>() 
            where TNotification : IDomainEvent;
    }

    public interface Then<TSubscriberDataContract, out TNotification, TEndpoint>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
    {
        ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint> Then(
            Action<TSubscriberDataContract, TNotification, TEndpoint> handler);
    }

    public class ConsumerBuilder<TSubscriberDataContract, TEndpoint> :
        Given<TSubscriberDataContract, TEndpoint>,
        When<TSubscriberDataContract, TEndpoint>
        where TSubscriberDataContract : new()
    {
        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ExportersBySubscription<TEndpoint>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ExportersBySubscription<TEndpoint>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> When<TNotification>()
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>(),
                new ExportersBySubscription<TEndpoint>()
            );
        }
    }

    class ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> : CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _subscriberDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> _subscriberDataMappers;
        public ExportersBySubscription<TEndpoint> ExportersBySubscription { get; }

        public ConsumerCorrelationMap(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> mappers,
            ExportersBySubscription<TEndpoint> exportersByNotificationContract)
        {
            _subscriberDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();

            _subscriberDataMappers = mappers 
                ?? new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>();

            ExportersBySubscription = exportersByNotificationContract ?? new ExportersBySubscription<TEndpoint>();
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint> Given<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ExportersBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint> When<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ExportersBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TEndpoint> When<TNotification1>()
            where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint> Then(
            Action<TSubscriberDataContract, TNotification, TEndpoint> handler)
        {
            ExportersBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(TSubscriberDataContract).Contract()),
                (notification, queryNotificationsByCorrelations, clock, endpoint) => Functions.BuildConsumer
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

        public CorrelationMap<TSubscriberDataContract, TNotification, TEndpoint> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right)
        {
            _subscriberDataContractMaps.Add(Type<TSubscriberDataContract>.Correlates(right, left));
            return this;
        }
    }
}