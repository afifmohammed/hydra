using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public interface CorrelationMap<TSubscriberDataContract, TNotification> : 
        Given<TSubscriberDataContract>, 
        PublisherContractSubscriptions<TSubscriberDataContract>, 
        Then<TSubscriberDataContract, TNotification>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
    {
        CorrelationMap<TSubscriberDataContract, TNotification> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right);
    }

    public interface Given<TSubscriberDataContract>
        where TSubscriberDataContract : new()
    {
        CorrelationMap<TSubscriberDataContract, TNotification> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;
    }

    public interface PublisherContractSubscriptions<TSubscriberDataContract> :
        When<TSubscriberDataContract>,
        PublisherSubscriptions
        where TSubscriberDataContract : new()
    { }

    public interface PublisherSubscriptions
    {
        PublishersBySubscription PublisherBySubscription { get; }
    }

    public interface When<TSubscriberDataContract>
        where TSubscriberDataContract : new()
    {
        CorrelationMap<TSubscriberDataContract, TNotification> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent;

        CorrelationMap<TSubscriberDataContract, TNotification> When<TNotification>() 
            where TNotification : IDomainEvent;
    }

    public interface Then<TSubscriberDataContract, out TNotification>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
    {
        PublisherContractSubscriptions<TSubscriberDataContract> Then(
            Func<TSubscriberDataContract, TNotification, IEnumerable<IDomainEvent>> handler);
    }

    public class PublisherBuilder<TSubscriberDataContract> :
        Given<TSubscriberDataContract>,
        When<TSubscriberDataContract>
        where TSubscriberDataContract : new()
    {
        public CorrelationMap<TSubscriberDataContract, TNotification> Given<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new PublisherCorrelationMap<TSubscriberDataContract, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new PublishersBySubscription()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification> When<TNotification>(
            Func<TNotification, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification : IDomainEvent
        {
            return new PublisherCorrelationMap<TSubscriberDataContract, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> { Type<TSubscriberDataContract>.Maps(mapper) },
                new PublishersBySubscription()
            );
        }

        public CorrelationMap<TSubscriberDataContract, TNotification> When<TNotification>() 
            where TNotification : IDomainEvent
        {
            return new PublisherCorrelationMap<TSubscriberDataContract, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>(),
                new PublishersBySubscription()
            );
        }
    }

    class PublisherCorrelationMap<TSubscriberDataContract, TNotification> : CorrelationMap<TSubscriberDataContract, TNotification>
        where TSubscriberDataContract : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _publisherDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> _publisherDataMappers;
        public PublishersBySubscription PublisherBySubscription { get; }

        public PublisherCorrelationMap(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>> mappers,
            PublishersBySubscription publisherByNotificationAndPublisherContract)
        {
            _publisherDataContractMaps = maps 
                ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();

            _publisherDataMappers = mappers 
                ?? new List<KeyValuePair<TypeContract, Func<TSubscriberDataContract, JsonContent, TSubscriberDataContract>>>();

            PublisherBySubscription = publisherByNotificationAndPublisherContract ?? new PublishersBySubscription();
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1> Given<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new PublisherCorrelationMap<TSubscriberDataContract, TNotification1>(
                _publisherDataContractMaps, 
                _publisherDataMappers, 
                PublisherBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1> When<TNotification1>(
            Func<TNotification1, TSubscriberDataContract, TSubscriberDataContract> mapper) 
            where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<TSubscriberDataContract>.Maps(mapper));

            return new PublisherCorrelationMap<TSubscriberDataContract, TNotification1>(
                _publisherDataContractMaps, 
                _publisherDataMappers, 
                PublisherBySubscription);
        }

        public CorrelationMap<TSubscriberDataContract, TNotification1> When<TNotification1>() 
            where TNotification1 : IDomainEvent
        {
            return new PublisherCorrelationMap<TSubscriberDataContract, TNotification1>(
                _publisherDataContractMaps,
                _publisherDataMappers,
                PublisherBySubscription);
        }

        public PublisherContractSubscriptions<TSubscriberDataContract> Then(
            Func<TSubscriberDataContract, TNotification, IEnumerable<IDomainEvent>> handler)
        {
            PublisherBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(TSubscriberDataContract).Contract()),
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

        public CorrelationMap<TSubscriberDataContract, TNotification> Correlate(
            Expression<Func<TNotification, object>> left, 
            Expression<Func<TSubscriberDataContract, object>> right)
        {
            _publisherDataContractMaps.Add(Type<TSubscriberDataContract>.Correlates(right, left));
            return this;
        }
    }
}
