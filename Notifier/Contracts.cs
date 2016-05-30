using System.Collections.Generic;
using Hydra.Core;

namespace Hydra.Notifier
{
    public class EventName : Wrapper<string>
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
