using System;

namespace EventSourcing
{
    public delegate NotificationsByPublisher Publisher(
        IDomainEvent notification,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock);

    public delegate void Consumer<in TEndpoint>(
        IDomainEvent notification,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint connection);

    public delegate void Consumer<in TEndpoint1, in TEndpoint2>(
        IDomainEvent notification,
        NotificationsByCorrelations queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint1 endpoint1,
        TEndpoint2 endpoint2);
}