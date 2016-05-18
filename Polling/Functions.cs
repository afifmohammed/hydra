using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace Polling
{
    public static class Functions
    {        
        public static void Handle<TEndpointConnection>(            
            CommitWork<TEndpointConnection> commit,
            Func<TEndpointConnection, LastSeen> buildLastSeen,
            Func<TEndpointConnection, RecentNotifications> buildRecentNotifications,
            IEnumerable<TypeContract> contracts,
            Action<IEnumerable<IDomainEvent>> publish,
            Func<TEndpointConnection, RecordLastSeen> buildRecordLastSeen) 
            where TEndpointConnection : EndpointConnection
        {
            commit
            (
                endpoint =>
                {
                    var lastSeen = Consume
                    (
                        lastSeen: buildLastSeen(endpoint),
                        recentNotifications: buildRecentNotifications(endpoint),
                        contracts: contracts,
                        publish: publish
                    );
                    var recordLastSeen = buildRecordLastSeen(endpoint);
                    var id = lastSeen();
                    recordLastSeen(id);
                }
            );
        }

        static LastSeen Consume(
            LastSeen lastSeen,
            RecentNotifications recentNotifications,
            IEnumerable<TypeContract> contracts,
            Action<IEnumerable<IDomainEvent>> publish)
        {
            return () => PublishAndReturnLastSeen
            (
                recentNotifications
                (
                    lastSeen(),
                    contracts.Select(x => new EventName { Value = x.Value }).ToArray()
                ),
                publish
            );
        }

        static EventId PublishAndReturnLastSeen(
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
