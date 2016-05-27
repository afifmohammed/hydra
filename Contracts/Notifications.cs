using System;
using System.Collections.Generic;

namespace Hydra.Core
{
    public class EventualConsistencyException<TEvent> : Exception
        where TEvent : IDomainEvent
    { }

    public class NoEventId : EventId { }

    public class Event
    {
        public EventId EventId { get; set; }
        public INotification Notification { get; set; }
    }

    public class EventId : Wrapper<long>
    {
        public long Value
        {
            get; set;
        }
    }

    public interface INotification : ICorrelated { }

    public interface IDomainEvent : INotification
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