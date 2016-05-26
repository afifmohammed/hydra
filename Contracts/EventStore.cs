using System;
using System.Collections.Generic;

namespace Hydra.Core
{
    public delegate IEnumerable<SerializedNotification> NotificationsByCorrelations(
        IEnumerable<Correlation> correlation);

    public delegate Func<IEnumerable<Correlation>, int> PublisherVersionByCorrelationsFunction<in TProvider>(
        TProvider provider)
        where TProvider : IProvider;

    public delegate NotificationsByCorrelations NotificationsByCorrelationsFunction<in TProvider>(
        TProvider provider)
        where TProvider : IProvider;

    public delegate Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersionAction<in TProvider>(
        TProvider provider)
        where TProvider : IProvider;
}