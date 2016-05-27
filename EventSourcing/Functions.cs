using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydra.Core
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

        public static Action<TNotification> BuildExporter<TConsumerData, TNotification, TExportProvider>(
            Action<TConsumerData, TNotification, TExportProvider> consumer,
            IDictionary<TypeContract, IReadOnlyCollection<CorrelationMap>> correlationMapsByConsumerDataContract,
            NotificationsByCorrelations notificationsByCorrelations,
            TExportProvider exportProvider,
            IDictionary<TypeContract, Func<TConsumerData, JsonContent, TConsumerData>> consumerDataMappersByNotificationContract,
            Func<DateTimeOffset> clock)
            where TConsumerData : new()
            where TNotification : IDomainEvent
        {
            return notification =>
            {
                var handlerDataCorrelationMaps = correlationMapsByConsumerDataContract[typeof(TConsumerData).Contract()];
                consumer
                (
                    FoldHandlerData
                    (
                        MapsWithMappers(handlerDataCorrelationMaps, consumerDataMappersByNotificationContract),
                        HandlerDataCorrelationsBy(handlerDataCorrelationMaps, notification),
                        notificationsByCorrelations,
                        consumerDataMappersByNotificationContract,
                        new TConsumerData(),
                        new NoEventId()
                    ),
                    notification,
                    exportProvider
                );
            };
        }

        public static Action<TNotification> BuildIntegrator<TConsumerData, TNotification, TLeftProvider, TRightProvider>(
            Action<TConsumerData, TNotification, TLeftProvider, TRightProvider> consumer,
            IDictionary<TypeContract, IReadOnlyCollection<CorrelationMap>> correlationMapsByConsumerDataContract,
            NotificationsByCorrelations notificationsByCorrelations,
            TLeftProvider leftProvider,
            TRightProvider rightProvider,
            IDictionary<TypeContract, Func<TConsumerData, JsonContent, TConsumerData>> consumerDataMappersByNotificationContract,
            Func<DateTimeOffset> clock)
            where TConsumerData : new()
            where TNotification : IDomainEvent
        {
            return notification =>
            {
                var handlerDataCorrelationMaps = correlationMapsByConsumerDataContract[typeof(TConsumerData).Contract()];

                consumer
                (
                    FoldHandlerData
                    (
                        MapsWithMappers(handlerDataCorrelationMaps, consumerDataMappersByNotificationContract),
                        HandlerDataCorrelationsBy(handlerDataCorrelationMaps, notification),
                        notificationsByCorrelations,
                        consumerDataMappersByNotificationContract,
                        new TConsumerData(),
                        new NoEventId()
                    ),
                    notification,
                    leftProvider,
                    rightProvider
                );
            };
        }

        public static Func<Event, NotificationsByPublisher> BuildPublisher<TPublisherData, TNotification>(
            Func<TPublisherData, TNotification, IEnumerable<IDomainEvent>> publisher,
            IDictionary<TypeContract, IReadOnlyCollection<CorrelationMap>> correlationMapsByPublisherDataContract,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<IDomainEvent, IEnumerable<Correlation>> correlationsByNotification,
            IDictionary<TypeContract, Func<TPublisherData, JsonContent, TPublisherData>> publisherDataMappersByNotificationContract,
            Func<DateTimeOffset> clock)
            where TPublisherData : new()
            where TNotification : INotification
        {
            return @event =>
            {
                var handlerDataCorrelationMaps = correlationMapsByPublisherDataContract[typeof(TPublisherData).Contract()];
                var notificaion = (TNotification) @event.Notification;
                return
                    new NotificationsByPublisher
                    {
                        Notifications = publisher
                        (
                            FoldHandlerData
                            (
                                MapsWithMappers(handlerDataCorrelationMaps, publisherDataMappersByNotificationContract),
                                HandlerDataCorrelationsBy(handlerDataCorrelationMaps, notificaion),
                                notificationsByCorrelations,
                                publisherDataMappersByNotificationContract,
                                new TPublisherData(),
                                @event.EventId
                            ),
                            notificaion
                        ).Select
                        (
                            n => new Tuple<IDomainEvent, IEnumerable<Correlation>>
                            (
                                n,
                                n.Correlations()
                            )
                        ),
                        PublisherDataCorrelations = HandlerDataCorrelationsBy
                        (
                            correlationMapsByPublisherDataContract[typeof (TPublisherData).Contract()],
                            notificaion
                        ),
                        When = clock()
                    };
            };
        }

        public static IReadOnlyCollection<CorrelationMap> MapsWithMappers<THandlerData>(
            IReadOnlyCollection<CorrelationMap> handlerDataCorrelationMaps,
            IDictionary<TypeContract, Func<THandlerData, JsonContent, THandlerData>>
                handlerDataMappersByNotificationContract)
        {
            return handlerDataCorrelationMaps
                .Where
                (
                    mapper => handlerDataMappersByNotificationContract
                        .Keys
                        .Any(notificationContract => notificationContract.Equals(mapper.NotificationContract))
                ).ToList().AsReadOnly();
        }

        public static THandlerData FoldHandlerData<THandlerData>(
            IReadOnlyCollection<CorrelationMap> handlerDataCorrelationMaps,
            IEnumerable<Correlation> handlerDataCorrelations,
            NotificationsByCorrelations notificationsByCorrelations,
            IDictionary<TypeContract, Func<THandlerData, JsonContent, THandlerData>> handlerDataMappersByNotificationContract,
            THandlerData handlerData,
            EventId eventId)
        {
            if (!handlerDataCorrelationMaps.Any())
                return handlerData;

            var correlations = CorrelationsOfMatchingNotificationsBy
            (
                handlerDataCorrelationMaps: handlerDataCorrelationMaps,
                handlerDataCorrelations: handlerDataCorrelations
            ).ToList();

            if (!correlations.Any())
                return handlerData;

            var notifications = notificationsByCorrelations(correlations, eventId).ToList();

            if (!notifications.Any())
                return handlerData;

            var content = new JsonContent(handlerData);

            handlerData = FoldHandlerData
            (
                handlerDataMappersByNotificationContract,
                notifications,
                handlerData
            );

            if(new JsonContent(handlerData).Equals(content))
                return handlerData;

            var unEvaluatedMaps = handlerDataCorrelationMaps
                .Where
                (
                    map => correlations
                        .GroupBy(c => c.Contract)
                        .Select(c => c.Key)
                        .All(notificationContract => !notificationContract.Equals(map.NotificationContract))
                ).ToList();

            return FoldHandlerData
            (
                unEvaluatedMaps,
                HandlerDataCorrelationsByHandlerData(unEvaluatedMaps, handlerData),
                notificationsByCorrelations,
                handlerDataMappersByNotificationContract,
                handlerData,
                new NoEventId()
            );
        }

        public static IEnumerable<Correlation> CorrelationsOfMatchingNotificationsBy(
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            IEnumerable<Correlation> handlerDataCorrelations)
        {
            return handlerDataCorrelationMaps
                .GroupBy(map => map.NotificationContract)
                .Where
                (
                    group => group.All
                    (
                        map => handlerDataCorrelations.Any
                        (
                            correlation =>
                                map.HandlerDataPropertyName == correlation.PropertyName
                                && !string.IsNullOrEmpty(correlation.PropertyValue.Value)
                        )
                    )
                )
                .SelectMany(group => group)
                .Select(map =>
                {
                    var values = handlerDataCorrelations.Where(x => x.PropertyName == map.HandlerDataPropertyName).ToList();
                    if(!values.Any())
                        throw new InvalidOperationException($"{map.HandlerDataContract.Value} does not correlate from {map.HandlerDataPropertyName} to {map.NotificationPropertyName} for {map.NotificationContract.Value}");

                    return new Correlation
                    {
                        PropertyName = map.NotificationPropertyName,
                        Contract = map.NotificationContract,
                        PropertyValue = values.First().PropertyValue
                    };

                });
        }

        public static IEnumerable<Correlation> HandlerDataCorrelationsBy<TNotification>(
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            TNotification notification) 
            where TNotification : INotification
        {
            return handlerDataCorrelationMaps
                .Where(m => m.NotificationContract.Equals(typeof(TNotification).Contract()))
                .Select(m => new Correlation
                {
                    PropertyName = m.HandlerDataPropertyName,
                    Contract = m.HandlerDataContract,
                    PropertyValue = new Lazy<string>(() => (m.NotificationPropertyName.GetPropertyValue(notification))?.ToString())
                }); 
        }

        public static IEnumerable<Correlation> HandlerDataCorrelationsByHandlerData<THandlerData>(
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            THandlerData handlerData)
        {
            return handlerDataCorrelationMaps
                .Select(m => new Correlation
                {
                    PropertyName = m.HandlerDataPropertyName,
                    Contract = m.HandlerDataContract,
                    PropertyValue = new Lazy<string>(() => (m.HandlerDataPropertyName.GetPropertyValue(handlerData))?.ToString())
                });
        }

        public static THandlerData FoldHandlerData<THandlerData>(
            IDictionary<TypeContract, Func<THandlerData, JsonContent, THandlerData>> handlerDataMappersByNotificationContract,
            IEnumerable<SerializedNotification> notifications,
            THandlerData handlerData) 
        {
            return notifications.Aggregate
            (
                handlerData,
                (current, notification)
                    =>
                    {
                        Func<THandlerData, JsonContent, THandlerData> mapper;
                        if(!handlerDataMappersByNotificationContract.TryGetValue(notification.Contract, out mapper))
                            throw new InvalidOperationException($"No mapper provided for mapping {notification.Contract.Value} into {typeof(THandlerData).Contract().Value}");

                        return handlerDataMappersByNotificationContract[notification.Contract]
                        (
                            current,
                            notification.JsonContent
                        );
                    }
            );
        }
    }


}
