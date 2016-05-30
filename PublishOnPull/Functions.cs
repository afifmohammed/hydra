using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;

namespace Hydra.PublishOnPull
{
    public static class Functions
    {
        public static void Handle<TStreamProvider, TStateProvider>(            
            CommitWork<TStateProvider> commitState,
            CommitWork<TStreamProvider> commitStream,
            Func<TStateProvider, LastSeen> lastSeenFunction,
            Func<TStreamProvider, RecentNotifications> recentNotificationsFunction,
            IEnumerable<TypeContract> contracts,
            Action<IEnumerable<IDomainEvent>> publish,
            Func<TStateProvider, RecordLastSeen> recordLastSeenFunction) 
            where TStreamProvider : IUowProvider
            where TStateProvider : IUowProvider
        {
            commitState
            (
                stateProvider => commitStream
                (
                    streamProvider => ConsumeAndRecordLastSeen
                    (
                        lastSeenFunction(stateProvider),
                        recentNotificationsFunction(streamProvider),
                        contracts,
                        publish,
                        recordLastSeenFunction(stateProvider)
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
