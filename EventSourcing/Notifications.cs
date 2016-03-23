using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    class BuildNotificationLog
    {
        public static IEnumerable<NotificationLog> For(
            IEnumerable<PublishedNotification> notifications,
            Func<IEnumerable<Correlation>, int> versionByPublisherDataCorrelations)
        {
            return notifications
                .Select(n => new NotificationLog
                {
                    NotificationCorrelations = n.NotificationCorrelations,
                    Notification = n.Content,
                    When = DateTimeOffset.Now,
                    ExpectedVersion = new Version(versionByPublisherDataCorrelations(n.PublisherDataCorrelations))
                }).Select(n => new NotificationLog
                {
                    Notification = n.Notification,
                    When = n.When,
                    ExpectedVersion = n.ExpectedVersion,
                    NotificationCorrelations = n.NotificationCorrelations,
                    Version = new Version(n.ExpectedVersion.Value + 1)
                });
        }
    }

    public class SerializedNotification
    {
        public TypeIdentifier Contract { get; set; }
        public JsonContent JsonContent { get; set; }
    }

    public class PublishedNotification
    {
        public IEnumerable<Correlation> PublisherDataCorrelations { get; set; }
        public IDomainEvent Content { get; set; }
        public IEnumerable<Correlation> NotificationCorrelations { get; set; }
    }

    public class NotificationLog
    {
        public IDomainEvent Notification { get; set; }
        public Version ExpectedVersion { get; set; }
        public Version Version { get; set; }
        public IEnumerable<Correlation> NotificationCorrelations { get; set; }
        public DateTimeOffset When { get; set; }
    }
}
