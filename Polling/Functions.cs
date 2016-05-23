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
            where TStreamConnection : IProvider
            where TStateConnection : IProvider
        {
            commitState
            (
                stateEndpointConnection => commitStream
                (
                    streamEndpointConnection => ConsumeAndRecordLastSeen
                    (
                        lastSeenFunction(stateEndpointConnection),
                        recentNotificationsFunction(streamEndpointConnection),
                        contracts,
                        publish,
                        recordLastSeenFunction(stateEndpointConnection)
                    )
                )
            );
        }

        static void ConsumeAndRecordLastSeen(
            LastSeen lastSeen,
            RecentNotifications recentNotifications,
            IEnumerable<TypeContract> contracts,
            Action<IEnumerable<IDomainEvent>> publish,
            RecordLastSeen recordLastSeen)
        {
            var id = Consume
            (
                lastSeen: lastSeen,
                recentNotifications: recentNotifications,
                contracts: contracts,
                publish: publish
            )();
            
            recordLastSeen(id);
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
