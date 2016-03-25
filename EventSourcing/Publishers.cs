using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class BuildPublisher
    {
        public static Func<TNotification, NotificationsByPublisher> For<TPublisherData, TNotification>(
            Func<TPublisherData, TNotification, IEnumerable<IDomainEvent>> publisher,
            Func<TypeIdentifier, IEnumerable<CorrelationMap>> correlationMapsByPublisherDataContract,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByPublisherDataCorrelations,
            Func<TypeIdentifier, IEnumerable<Correlation>> correlationsByNotificationContract,
            IDictionary<TypeIdentifier, Func<TPublisherData, JsonContent, TPublisherData>> publisherDataBuildersByNotificationContract)
            where TPublisherData : class, new()
            where TNotification : IDomainEvent
        {
            return notification => new NotificationsByPublisher
            {
                Notifications = publisher
                (
                    Load
                    (
                        notification,
                        correlationMapsByPublisherDataContract(TypeIdentifier.For<TPublisherData>()),
                        notificationsByPublisherDataCorrelations,
                        publisherDataBuildersByNotificationContract
                    ),
                    notification
                ).Select
                (
                    n => new Tuple<IDomainEvent, IEnumerable<Correlation>>
                    (
                        n, 
                        BuildCorrelationsFor.CorrelatedNotificationsBy
                        (
                            correlationMapsByPublisherDataContract(TypeIdentifier.For<TPublisherData>()),
                            correlationsByNotificationContract(new TypeIdentifier(n))
                        ).Where(x => x.Contract.Equals(new TypeIdentifier(n)))
                    )
                )
            };            
        }

        public static THandlerData Load<THandlerData, TNotification>(
            TNotification notification,
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            IDictionary<TypeIdentifier, Func<THandlerData, JsonContent, THandlerData>> handlerDataBuildersByNotificationContract) 
                where THandlerData : class, new()
                where TNotification : IDomainEvent
        {
            return Build
            (
                handlerDataBuildersByNotificationContract, 
                notificationsByCorrelations
                (
                    BuildCorrelationsFor.CorrelatedNotificationsBy
                    (
                        handlerDataCorrelationMaps,
                        BuildCorrelationsFor.HandlerDataBy(handlerDataCorrelationMaps, notification)                        
                    )
                )
            );
        }

        public static THandlerData Build<THandlerData>(
            IDictionary<TypeIdentifier, Func<THandlerData, JsonContent, THandlerData>> handlerDataMappersByNotificationContract,
            IEnumerable<SerializedNotification> notifications) where THandlerData : class, new()
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
