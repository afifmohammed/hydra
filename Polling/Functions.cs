using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace Polling
{
    public static class Functions
    {        
        public static void Handle<TStreamConnection, TStateConnection>(            
            CommitWork<TStateConnection> commitState,
            CommitWork<TStreamConnection> commitStream,
            Func<TStateConnection, LastSeen> lastSeenFunction,
            Func<TStreamConnection, RecentNotifications> recentNotificationsFunction,
            IEnumerable<TypeContract> contracts,
            Action<IEnumerable<IDomainEvent>> publish,
            Func<TStateConnection, RecordLastSeen> recordLastSeenFunction) 
            where TStreamConnection : EndpointConnection
            where TStateConnection : EndpointConnection
        {
            commitState
            (
                stateEndpointConnection =>
                {
                    commitStream
                    (
                        streamEndpointConnection =>
                        {
                            var lastSeen = Consume
                            (
                                lastSeen: lastSeenFunction(stateEndpointConnection),
                                recentNotifications: recentNotificationsFunction(streamEndpointConnection),
                                contracts: contracts,
                                publish: publish
                            );
                            var recordLastSeen = recordLastSeenFunction(stateEndpointConnection);
                            var id = lastSeen();
                            recordLastSeen(id);
                        }
                    );                    
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
