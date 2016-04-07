using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class Functions
    {
        public static NotificationsByPublisherAndVersion AppendPublisherVersion(
            NotificationsByPublisher notifications,
            Func<IEnumerable<Correlation>, int> versionByPublisherDataCorrelations)
        {
            var v = versionByPublisherDataCorrelations(notifications.PublisherDataCorrelations);
            return new NotificationsByPublisherAndVersion
            {
                NotificationsByPublisher = notifications,
                ExpectedVersion = new Version(v),
                Version = new Version(v + 1)
            };
        }

        public static Action<TNotification> BuildSubscriber<TSubscriberData, TNotification, TEndpoint>(
            Action<TSubscriberData, TNotification, TEndpoint> subscriber,
            IDictionary<TypeContract, IEnumerable<CorrelationMap>> correlationMapsBySubscriberDataContract,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            TEndpoint endpoint,
            IDictionary<TypeContract, Func<TSubscriberData, JsonContent, TSubscriberData>> subscriberDataMappersByNotificationContract,
            Func<DateTimeOffset> clock)
            where TSubscriberData : new()
            where TNotification : IDomainEvent
        {
            return notification => subscriber
            (
                FoldHandlerData
                (
                    notification,
                    correlationMapsBySubscriberDataContract[typeof(TSubscriberData).Contract()],
                    notificationsByCorrelations,
                    subscriberDataMappersByNotificationContract
                ),
                notification,
                endpoint
            );
        }

        public static Action<TNotification> BuildSubscriber<TSubscriberData, TNotification, TEndpoint1, TEndpoint2>(
            Action<TSubscriberData, TNotification, TEndpoint1, TEndpoint2> subscriber,
            IDictionary<TypeContract, IEnumerable<CorrelationMap>> correlationMapsBySubscriberDataContract,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            TEndpoint1 endpoint1,
            TEndpoint2 endpoint2,
            IDictionary<TypeContract, Func<TSubscriberData, JsonContent, TSubscriberData>> subscriberDataMappersByNotificationContract,
            Func<DateTimeOffset> clock)
            where TSubscriberData : new()
            where TNotification : IDomainEvent
        {
            return notification => subscriber
            (
                FoldHandlerData
                (
                    notification,
                    correlationMapsBySubscriberDataContract[typeof(TSubscriberData).Contract()],
                    notificationsByCorrelations,
                    subscriberDataMappersByNotificationContract
                ),
                notification,
                endpoint1,
                endpoint2
            );
        }

        public static Func<TNotification, NotificationsByPublisher> BuildPublisher<TPublisherData, TNotification>(
            Func<TPublisherData, TNotification, IEnumerable<IDomainEvent>> publisher,
            IDictionary<TypeContract, IEnumerable<CorrelationMap>> correlationMapsByPublisherDataContract,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<IDomainEvent, IEnumerable<Correlation>> correlationsByNotification,
            IDictionary<TypeContract, Func<TPublisherData, JsonContent, TPublisherData>> publisherDataMappersByNotificationContract,
            Func<DateTimeOffset> clock)
            where TPublisherData : new()
            where TNotification : IDomainEvent
        {
            return notification => new NotificationsByPublisher
            {
                Notifications = publisher
                (
                    FoldHandlerData
                    (
                        notification,
                        correlationMapsByPublisherDataContract[typeof(TPublisherData).Contract()],
                        notificationsByCorrelations,
                        publisherDataMappersByNotificationContract
                    ),
                    notification
                ).Select
                (
                    n => new Tuple<IDomainEvent, IEnumerable<Correlation>>
                    (
                        n,
                        CorrelationsOfMatchingNotificationsBy
                        (
                            correlationMapsByPublisherDataContract[typeof(TPublisherData).Contract()],
                            correlationsByNotification(n)
                        ).Where(x => x.Contract.Equals(new TypeContract(n)))
                    )
                ),
                PublisherDataCorrelations = HandlerDataCorrelationsBy(correlationMapsByPublisherDataContract[typeof(TPublisherData).Contract()], notification),
                When = clock()
            };
        }

        public static THandlerData FoldHandlerData<THandlerData, TNotification>(
            TNotification notification,
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            IDictionary<TypeContract, Func<THandlerData, JsonContent, THandlerData>> handlerDataMappersByNotificationContract)
                where THandlerData : new()
                where TNotification : IDomainEvent
        {
            return FoldHandlerData
            (
                handlerDataMappersByNotificationContract,
                notificationsByCorrelations
                (
                    CorrelationsOfMatchingNotificationsBy
                    (
                        handlerDataCorrelationMaps,
                        HandlerDataCorrelationsBy(handlerDataCorrelationMaps, notification)
                    )
                )
            );
        }

        public static IEnumerable<Correlation> CorrelationsOfMatchingNotificationsBy(
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            IEnumerable<Correlation> handlerDataCorrelations)
        {
            return handlerDataCorrelationMaps.Select(map => new Correlation
            {
                PropertyName = map.NotificationPropertyName,
                Contract = map.NotificationContract,
                PropertyValue = handlerDataCorrelations.Single(x => x.PropertyName == map.HandlerDataPropertyName).PropertyValue
            });
        }

        public static IEnumerable<Correlation> HandlerDataCorrelationsBy<TNotification>(
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            TNotification notification)
        {
            return handlerDataCorrelationMaps
                .Where(m => m.NotificationContract.Equals(typeof(TNotification).Contract()))
                .Select(m => new Correlation
                {
                    PropertyName = m.HandlerDataPropertyName,
                    Contract = m.HandlerDataContract,
                    PropertyValue = new Lazy<string>(() => (m.NotificationPropertyName.GetPropertySelector<TNotification>().Compile()(notification)).ToString())
                }); 
        }

        public static THandlerData FoldHandlerData<THandlerData>(
            IDictionary<TypeContract, Func<THandlerData, JsonContent, THandlerData>> handlerDataMappersByNotificationContract,
            IEnumerable<SerializedNotification> notifications) where THandlerData : new()
        {
            var handlerData = new THandlerData();

            return notifications.Aggregate(handlerData, (current, notification) => handlerDataMappersByNotificationContract[notification.Contract](current, notification.JsonContent));
        }
    }


}
