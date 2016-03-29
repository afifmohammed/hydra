using System;
using System.Collections.Generic;

namespace EventSourcing
{
    public static class GlobalConfiguration
    {
        public static Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> NotificationsByCorrelations;
        public static Func<DateTimeOffset> Clock = () => DateTimeOffset.Now;
    }
}