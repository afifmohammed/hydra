using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class BuildPublisher
    {
        public static Func<TNotification, IEnumerable<PublishedNotification>> For<TPublisherData, TNotification>(
            Func<TPublisherData, TNotification, IEnumerable<IDomainEvent>> publisher,
            Func<TypeIdentifier, IEnumerable<CorrelationMap>> correlationMapsByPublisherDataContract,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByPublisherDataCorrelations,
            Func<TypeIdentifier, Func<TPublisherData, JsonContent, TPublisherData>> publisherDataBuildersByNotificationContract)
            where TPublisherData : class, new()
            where TNotification : IDomainEvent
        {
            return notification => publisher
            (
                Load
                (
                    notification,
                    correlationMapsByPublisherDataContract(TypeIdentifier.For<TPublisherData>()),
                    notificationsByPublisherDataCorrelations,
                    publisherDataBuildersByNotificationContract
                ),
                notification
            )
            .Select(n => new PublishedNotification
            {
                Content = n,
                PublisherDataCorrelations = BuildCorrelationsFor.HandlerDataBy(correlationMapsByPublisherDataContract(TypeIdentifier.For<TPublisherData>()), notification),
                NotificationCorrelations = BuildCorrelationsFor.CorrelatedNotificationsBy
                (
                    correlationMapsByPublisherDataContract(TypeIdentifier.For<TPublisherData>()),
                    BuildCorrelationsFor.HandlerDataBy(correlationMapsByPublisherDataContract(TypeIdentifier.For<TPublisherData>()), notification)
                ).Where(x => x.Contract.Equals(new TypeIdentifier(n)))
            });
        }

        public static THandlerData Load<THandlerData, TNotification>(
            TNotification notification,
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<TypeIdentifier, Func<THandlerData, JsonContent, THandlerData>> handlerDataBuildersByNotificationContract) 
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
            Func<TypeIdentifier, Func<THandlerData, JsonContent, THandlerData>> handlerDataMappersByNotificationContract,
            IEnumerable<SerializedNotification> notifications) where THandlerData : class, new()
        {
            var handlerData = new THandlerData();

            foreach (var notification in notifications)
                handlerData = handlerDataMappersByNotificationContract
                    (notification.Contract)
                    (handlerData, notification.JsonContent);

            return handlerData;
        }
    }
}
