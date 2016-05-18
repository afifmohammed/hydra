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
            publish(notifications.Select(x => x.Event));
            return notifications.Reverse().First().Id;
        }
    }
}
