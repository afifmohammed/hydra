using System.Collections.Generic;
using EventSourcing;

namespace Polling
{
    public class NoEventId : EventId { }

    public class EventId : Unit<long>
    {
        public long Value
        {
            get;set;
        }
    }

    public class EventName : Unit<string>
    {
        public string Value
        {
            get;set;
        }
    }

    public class Notification
    {
        public EventId Id { get; set; }
        public IDomainEvent Event { get; set; }
    }

    public delegate IEnumerable<Notification> RecentNotifications(EventId id, EventName[] names);    

    public delegate void RecordLastSeen(EventId id);

    public delegate EventId LastSeen();    
}
