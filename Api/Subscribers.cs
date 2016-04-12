using System;
using System.Collections.Generic;

namespace EventSourcing
{
    public delegate NotificationsByPublisher Publisher(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock);

    public delegate void Consumer<in TEndpoint>(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint connection);

    public delegate void Consumer<in TEndpoint1, in TEndpoint2>(
        IDomainEvent notification,
        Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> queryNotificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TEndpoint1 endpoint1,
        TEndpoint2 endpoint2);
}