using System;
using System.Collections.Generic;

namespace Hydra.Core
{
    public delegate IEnumerable<SerializedNotification> NotificationsByCorrelations(
        IEnumerable<Correlation> correlation,
        EventId eventId);

    public delegate Func<IEnumerable<Correlation>, int> PublisherVersionByCorrelationsFunction<in TUowProvider>(
        TUowProvider provider)
        where TUowProvider : IUowProvider;

    public delegate NotificationsByCorrelations NotificationsByCorrelationsFunction<in TUowProvider>(
        TUowProvider provider)
        where TUowProvider : IUowProvider;

    public delegate Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersionAction<in TUowProvider>(
        TUowProvider provider)
        where TUowProvider : IUowProvider;
}