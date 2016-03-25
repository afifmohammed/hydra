using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    class AppendPublisherVersion
    {
        public static NotificationsByPublisherAndVersion To(
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
    }

    public class SerializedNotification
    {
        public TypeIdentifier Contract { get; set; }
        public JsonContent JsonContent { get; set; }
    }

    public class NotificationsByPublisher
    {
        public IEnumerable<Tuple<IDomainEvent, IEnumerable<Correlation>>> Notifications { get; set; }
        public IEnumerable<Correlation> PublisherDataCorrelations { get; set; }
        public DateTimeOffset When { get; set; }
    }

    public class NotificationsByPublisherAndVersion
    {
        public NotificationsByPublisher NotificationsByPublisher { get; set; }
        public Version Version { get; set; }
        public Version ExpectedVersion { get; set; }
    }
}
