using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace Polling
{
    public static class Functions
    {
        public static LastSeen Apply(
            LastSeen lastSeen,
            RecentNotifications recentNotifications,
            IEnumerable<TypeContract> contracts,
            Action<IEnumerable<IDomainEvent>> publish)
        {
            return () => Apply
            (
                recentNotifications
                (
                    lastSeen(),
                    contracts.Select(x => new EventName { Value = x.Value }).ToArray()
                ),
                publish
            );
        }

        static EventId Apply(
            IEnumerable<Notification> notifications,
            Action<IEnumerable<IDomainEvent>> publish)
        {
            EventId id = new NoEventId();

            publish(notifications.OrderBy(x => x.Id.Value).Select(x => 
            {
                id = x.Id;
                return x.Event;
            }));
            return id;
        }
    }
}
