using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public delegate NotificationsByPublisher Publisher(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock);

    public class PublishersBySubscription : Dictionary<Subscription, Publisher>
    { }

    public interface CorrelationMap<THandlerContract, TNotification> : Given<THandlerContract>, PublisherContractSubscriptions<THandlerContract>, Then<THandlerContract, TNotification>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        CorrelationMap<THandlerContract, TNotification> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<THandlerContract, object>> right);
    }

    public interface Given<THandlerContract>
        where THandlerContract : new()
    {
        CorrelationMap<THandlerContract, TNotification> Given<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent;
    }

    public struct Subscription
    {
        public Subscription(TypeContract notificationContract, TypeContract subscriberDataContract)
        {
            NotificationContract = notificationContract;
            SubscriberDataContract = subscriberDataContract;
        }

        public TypeContract NotificationContract { get; set; }
        public TypeContract SubscriberDataContract { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if ((obj is Subscription) == false) return false;

            return Equals((Subscription)obj);
        }

        public bool Equals(Subscription other)
        {
            return NotificationContract.Equals(other.NotificationContract) && SubscriberDataContract.Equals(other.SubscriberDataContract);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NotificationContract.GetHashCode() * 397) ^ SubscriberDataContract.GetHashCode();
            }
        }
    }

    public interface PublisherContractSubscriptions<THandlerContract> :
        When<THandlerContract>,
        PublisherSubscriptions
        where THandlerContract : new()
    { }

    public interface PublisherSubscriptions
    {
        PublishersBySubscription PublisherBySubscription { get; }
    }

    public interface When<THandlerContract>
        where THandlerContract : new()
    {
        CorrelationMap<THandlerContract, TNotification> When<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent;
        CorrelationMap<THandlerContract, TNotification> When<TNotification>() where TNotification : IDomainEvent;
    }

    public interface Then<THandlerContract, out TNotification>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        PublisherContractSubscriptions<THandlerContract> Then(Func<THandlerContract, TNotification, IEnumerable<IDomainEvent>> handler);
    }

    public class PublisherBuilder<THandlerContract> :
        Given<THandlerContract>,
        When<THandlerContract>
        where THandlerContract : new()
    {
        public CorrelationMap<THandlerContract, TNotification> Given<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> { Type<THandlerContract>.Maps(mapper) },
                new PublishersBySubscription()
            );
        }

        public CorrelationMap<THandlerContract, TNotification> When<TNotification>(Func<TNotification, THandlerContract, THandlerContract> mapper) where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> { Type<THandlerContract>.Maps(mapper) },
                new PublishersBySubscription()
            );
        }

        public CorrelationMap<THandlerContract, TNotification> When<TNotification>() where TNotification : IDomainEvent
        {
            return new NotificationHandler<THandlerContract, TNotification>
            (
                new List<KeyValuePair<TypeContract, CorrelationMap>>(),
                new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>>(),
                new PublishersBySubscription()
            );
        }
    }

    class NotificationHandler<THandlerContract, TNotification> : CorrelationMap<THandlerContract, TNotification>
        where THandlerContract : new()
        where TNotification : IDomainEvent
    {
        readonly List<KeyValuePair<TypeContract, CorrelationMap>> _publisherDataContractMaps;
        readonly List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> _publisherDataMappers;
        public PublishersBySubscription PublisherBySubscription { get; }

        public NotificationHandler(
            List<KeyValuePair<TypeContract, CorrelationMap>> maps,
            List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>> mappers,
            PublishersBySubscription publisherByNotificationAndPublisherContract)
        {
            _publisherDataContractMaps = maps ?? new List<KeyValuePair<TypeContract, CorrelationMap>>();
            _publisherDataMappers = mappers ?? new List<KeyValuePair<TypeContract, Func<THandlerContract, JsonContent, THandlerContract>>>();

            PublisherBySubscription = publisherByNotificationAndPublisherContract ?? new PublishersBySubscription();
        }

        public CorrelationMap<THandlerContract, TNotification1> Given<TNotification1>(Func<TNotification1, THandlerContract, THandlerContract> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<THandlerContract>.Maps(mapper));
            return new NotificationHandler<THandlerContract, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, PublisherBySubscription);
        }

        public CorrelationMap<THandlerContract, TNotification1> When<TNotification1>(Func<TNotification1, THandlerContract, THandlerContract> mapper) where TNotification1 : IDomainEvent
        {
            _publisherDataMappers.Add(Type<THandlerContract>.Maps(mapper));
            return new NotificationHandler<THandlerContract, TNotification1>(_publisherDataContractMaps, _publisherDataMappers, PublisherBySubscription);
        }

        public CorrelationMap<THandlerContract, TNotification1> When<TNotification1>() where TNotification1 : IDomainEvent
        {
            return When<TNotification1>((e, d) => d);
        }

        public PublisherContractSubscriptions<THandlerContract> Then(Func<THandlerContract, TNotification, IEnumerable<IDomainEvent>> handler)
        {
            PublisherBySubscription.Add
            (
                new Subscription(typeof(TNotification).Contract(), typeof(THandlerContract).Contract()),
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

        public CorrelationMap<THandlerContract, TNotification> Correlate(Expression<Func<TNotification, object>> left, Expression<Func<THandlerContract, object>> right)
        {
            _publisherDataContractMaps.Add(Type<THandlerContract>.Correlates(right, left));
            return this;
        }
    }
}
