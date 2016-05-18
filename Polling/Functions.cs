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
            Consume consume, 
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
                endpoint => Apply
                (
                    consume
                    (
                        lastSeen(endpoint), 
                        recentNotifications(endpoint), 
                        contracts, 
                        publish
                    ), 
                    recordLastSeen(endpoint)
                )
            );
        }

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

        static void Apply(LastSeen lastSeen, RecordLastSeen recordLastSeen)
        {
            recordLastSeen(lastSeen());
        }
    }
}
