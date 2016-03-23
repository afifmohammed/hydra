using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class BuildPublisher
    {
        public static IEnumerable<ICommand> Map(
            IEnumerable<Notification> notifications,
            Func<IEnumerable<Correlation>, int> versionByPublisherDataCorrelations)
        {

            return Enumerable.Empty<ICommand>();
        }

        public static Func<TNotification, IEnumerable<Notification>> For<TPublisherData, TNotification>(
            Func<TPublisherData, TNotification, IEnumerable<IDomainEvent>> publisher,
            Func<Contract, IEnumerable<CorrelationMap>> correlationMapsByPublisherDataContract,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByPublisherDataCorrelations,
            Func<Contract, Func<TPublisherData, JsonContent, TPublisherData>> publisherDataBuildersByNotificationContract)
            where TPublisherData : class, new()
            where TNotification : IDomainEvent
        {
            return notification => publisher
            (
                Load
                (
                    notification,
                    correlationMapsByPublisherDataContract(Contract.For<TPublisherData>()),
                    notificationsByPublisherDataCorrelations,
                    publisherDataBuildersByNotificationContract
                ),
                notification
            )
            .Select(n => new Notification
            {
                Content = n,
                PublisherDataCorrelations = CorrelationBuilder.CorrelationsBy(correlationMapsByPublisherDataContract(Contract.For<TPublisherData>()), notification)
            });
        }

        public static THandlerData Load<THandlerData, TNotification>(
            TNotification notification,
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> notificationsByCorrelations,
            Func<Contract, Func<THandlerData, JsonContent, THandlerData>> handlerDataBuildersByNotificationContract) 
                where THandlerData : class, new()
                where TNotification : IDomainEvent
        {
            return Build
            (
                handlerDataBuildersByNotificationContract, 
                notificationsByCorrelations
                (
                    CorrelationBuilder.CorrelationsBy
                    (
                        handlerDataCorrelationMaps, 
                        handlerDataCorrelations: CorrelationBuilder.CorrelationsBy(handlerDataCorrelationMaps, notification)
                    )
                )
            );
        }

        public static THandlerData Build<THandlerData>(
            Func<Contract, Func<THandlerData, JsonContent, THandlerData>> handlerDataMappersByNotificationContract,
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
