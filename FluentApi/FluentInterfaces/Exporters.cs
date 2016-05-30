using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hydra.Subscribers;

namespace Hydra.Core.FluentInterfaces
{
    public interface ConsumerContractSubscriptions<TSubscriberContract, TUowProvider> :
        ConsumerSubscriptions<TUowProvider>,
        When<TSubscriberContract, TUowProvider>
        where TSubscriberContract : new()
        where TUowProvider : IUowProvider
    { }

    public interface ConsumerSubscriptions<TUowProvider>
        where TUowProvider : IUowProvider
    {
        ProjectorsBySubscription<TUowProvider> ProjectorsBySubscription { get; }
    }

    public interface CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> :
        Given<TSubscriberDataContract, TUowProvider>,
        ConsumerContractSubscriptions<TSubscriberDataContract, TUowProvider>,
        Then<TSubscriberDataContract, TNotification, TUowProvider>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TUowProvider : IUowProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right);
    }

    public interface Given<TSubscriberDataContract, TUowProvider>
        where TSubscriberDataContract : new()
        where TUowProvider : IUowProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;
    }

    public interface When<TSubscriberDataContract, TUowProvider>
        where TSubscriberDataContract : new()
        where TUowProvider : IUowProvider
    {
        CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;

        CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> When<TNotification>() 
            where TNotification : IDomainEvent;
    }

    public interface Then<TSubscriberDataContract, out TNotification, TUowProvider>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TUowProvider : IUowProvider
    {
        ConsumerContractSubscriptions<TSubscriberDataContract, TUowProvider> Then(
            Action<TSubscriberDataContract, TNotification, TUowProvider> handler);
    }

    public class ConsumerBuilder<TSubscriberDataContract, TUowProvider> :
        Given<TSubscriberDataContract, TUowProvider>,
        When<TSubscriberDataContract, TUowProvider>
        where TSubscriberDataContract : new()
        where TUowProvider : IUowProvider
    {
        public CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TUowProvider>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ProjectorsBySubscription<TUowProvider>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TUowProvider>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new ProjectorsBySubscription<TUowProvider>()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> When<TNotification>()
            where TNotification : IDomainEvent
        {
            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TUowProvider>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>(),
                new ProjectorsBySubscription<TUowProvider>()
            );
        }
    }

    class ConsumerCorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> : CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
        where TUowProvider : IUowProvider
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _subscriberDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> _subscriberDataMappers;
        public ProjectorsBySubscription<TUowProvider> ProjectorsBySubscription { get; }

        public ConsumerCorrelationMap(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> mappers,
            ProjectorsBySubscription<TUowProvider> projectorsByNotificationContract)
        {
            _subscriberDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();

            _subscriberDataMappers = mappers 
                ?? new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>();

            ProjectorsBySubscription = projectorsByNotificationContract ?? new ProjectorsBySubscription<TUowProvider>();
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TUowProvider> Given<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TUowProvider>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ProjectorsBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TUowProvider> When<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _subscriberDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new ConsumerCorrelationMap<TSubscriberDataContract, TNotification1, TUowProvider>(
                _subscriberDataContractMaps, 
                _subscriberDataMappers, 
                ProjectorsBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1, TUowProvider> When<TNotification1>()
            where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public ConsumerContractSubscriptions<TSubscriberDataContract, TUowProvider> Then(
            Action<TSubscriberDataContract, TNotification, TUowProvider> handler)
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

        public CorrelationMap<TSubscriberDataContract, TNotification, TUowProvider> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right)
        {
            _subscriberDataContractMaps.Add(Type<TSubscriberDataContract>.Correlates(right, left));
            return this;
        }
    }
}