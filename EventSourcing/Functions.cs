using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    static class Functions
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

        public static Func<TNotification, NotificationsByPublisher> GroupNotificationsByPublisher<TPublisherData, TNotification>(
            Func<TPublisherData, TNotification, IEnumerable<IDomainEvent>> publisher,
            IDictionary<TypeContract, IEnumerable<CorrelationMap>> correlationMapsByPublisherDataContract,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByPublisherDataCorrelations,
            IDictionary<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>> correlationsByNotificationContract,
            IDictionary<TypeContract, Func<TPublisherData, JsonContent, TPublisherData>> publisherDataMappersByNotificationContract)
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
                        notificationsByPublisherDataCorrelations,
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
                            correlationsByNotificationContract[new TypeContract(n)](n)
                        ).Where(x => x.Contract.Equals(new TypeContract(n)))
                    )
                )
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
            return handlerDataCorrelationMaps.Select(m => new Correlation
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

            foreach (var notification in notifications)
                handlerData = handlerDataMappersByNotificationContract
                    [notification.Contract]
                    (handlerData, notification.JsonContent);

            return handlerData;
        }
    }


}
