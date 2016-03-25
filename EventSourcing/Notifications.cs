using System;
using System.Collections.Generic;

namespace EventSourcing
{
    public interface IDomainEvent
    { }

    public struct SerializedNotification
    {
        public TypeContract Contract { get; set; }
        public JsonContent JsonContent { get; set; }
    }

    public struct NotificationsByPublisher
    {
        public IEnumerable<Tuple<IDomainEvent, IEnumerable<Correlation>>> Notifications { get; set; }
        public IEnumerable<Correlation> PublisherDataCorrelations { get; set; }
        public DateTimeOffset When { get; set; }
    }

    public struct NotificationsByPublisherAndVersion
    {
        public NotificationsByPublisher NotificationsByPublisher { get; set; }
        public Version Version { get; set; }
        public Version ExpectedVersion { get; set; }
    }
}
