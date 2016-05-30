using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hydra.Subscribers;

namespace Hydra.Core.FluentInterfaces
{
    public interface ConsumerContractSubscriptions<TSubscriberContract, TProvider> :
        ConsumerSubscriptions<TProvider>,
        When<TSubscriberContract, TProvider>
        where TSubscriberContract : new()
        where TProvider : IProvider
    { }

    public interface ConsumerSubscriptions<TProvider>
        where TProvider : IProvider
    {
        ProjectorsBySubscription<TProvider> ProjectorsBySubscription { get; }
    }

    public interface CorrelationMap<TSubscriberDataContract, TNotification, TProvider> :
        Given<TSubscriberDataContract, TProvider>,
        ConsumerContractSubscriptions<TSubscriberDataContract, TProvider>,
        Then<TSubscriberDataContract, TNotification, TProvider>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TProvider : IProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TProvider> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right);
    }

    public interface Given<TSubscriberDataContract, TProvider>
        where TSubscriberDataContract : new()
        where TProvider : IProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TProvider> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;
    }

    public interface When<TSubscriberDataContract, TProvider>
        where TSubscriberDataContract : new()
        where TProvider : IProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TProvider> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;

        CorrelationMap<TSubscriberDataContract, TNotification, TProvider> When<TNotification>() 
            where TNotification : IDomainEvent;
    }

    public interface Then<TSubscriberDataContract, out TNotification, TProvider>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TProvider : IProvider
    {
        ConsumerContractSubscriptions<TSubscriberDataContract, TProvider> Then(
            Action<TSubscriberDataContract, TNotification, TProvider> handler);
    }

    public class ConsumerBuilder<TSubscriberDataContract, TProvider> :
        Given<TSubscriberDataContract, TProvider>,
        When<TSubscriberDataContract, TProvider>
        where TSubscriberDataContract : new()
        where TProvider : IProvider
    {
        public CorrelationMap<TSubscriberDataContract, TNotification, TProvider> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TProvider>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ProjectorsBySubscription<TProvider>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TProvider> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TProvider>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ProjectorsBySubscription<TProvider>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TProvider> When<TNotification>()
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TProvider>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>(),
                new ProjectorsBySubscription<TProvider>()
            );
        }
    }

    class ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TProvider> : CorrelationMap<TSubscriberDataContract, TNotification, TProvider>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TProvider : IProvider
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _subscriberDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> _subscriberDataMappers;
        public ProjectorsBySubscription<TProvider> ProjectorsBySubscription { get; }

        public ConsumerCorrelationMap(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> mappers,
            ProjectorsBySubscription<TProvider> projectorsByNotificationContract)
        {
            _subscriberDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();

            _subscriberDataMappers = mappers 
                ?? new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>();

            ProjectorsBySubscription = projectorsByNotificationContract ?? new ProjectorsBySubscription<TProvider>();
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TProvider> Given<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TProvider>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ProjectorsBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TProvider> When<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TProvider>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ProjectorsBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TProvider> When<TNotification1>()
            where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public ConsumerContractSubscriptions<TSubscriberDataContract, TProvider> Then(
            Action<TSubscriberDataContract, TNotification, TProvider> handler)
        {
            ProjectorsBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(TSubscriberDataContract).Contract()),
                (@event, queryNotificationsByCorrelations, clock, provider) => 
                    Functions.BuildExporter
                    (
                        handler,
                        _subscriberDataContractMaps.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => (IReadOnlyCollection<CorrelationMap>) x.Select(a => a.Value).ToList()),
                        queryNotificationsByCorrelations,
                        provider,
                        _subscriberDataMappers.ToDictionary(x => x.Key, x => x.Value),
                        clock
                    )((TNotification)@event.Notification)
            );

            return this;
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TProvider> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right)
        {
            _subscriberDataContractMaps.Add(Type<TSubscriberDataContract>.Correlates(right, left));
            return this;
        }
    }
}