using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace Polling
{
    public static class Builder<TEndpointConnection> where TEndpointConnection : EndpointConnection
    {
        public static Func<TEndpointConnection, LastSeen> LastSeen { get; set; }
        public static Func<TEndpointConnection, RecentNotifications> RecentNotifications { get; set; }
        public static Func<TEndpointConnection, RecordLastSeen> RecordLastSeen { get; set; }
        public static CommitWork<TEndpointConnection> CommitWork { get; set; }
    }

    public static class Functions
    {        
        public static void Apply<TEndpointConnection>(            
            CommitWork<TEndpointConnection> commit,
            Func<TEndpointConnection, LastSeen> lastSeen,
            Func<TEndpointConnection, RecentNotifications> recentNotifications,
            IEnumerable<TypeContract> contracts,
            Action<IEnumerable<IDomainEvent>> publish,
            Func<TEndpointConnection, RecordLastSeen> recordLastSeen
            ) where TEndpointConnection : EndpointConnection
        {
            commit
            (
                endpoint =>
                {
                    var last = Consume
                    (
                        lastSeen(endpoint),
                        recentNotifications(endpoint),
                        contracts,
                        publish
                    );
                    var id = last();
                    var record = recordLastSeen(endpoint);
                    record(id);
                }
            );
        }

        static LastSeen Consume(
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
