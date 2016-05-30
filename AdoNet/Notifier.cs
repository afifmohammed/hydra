using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;
using Hydra.Notifier;
using Hydra.Subscriptions;

namespace Hydra.AdoNet
{
    public static class SqlEventStoreNotifier<TEventStoreConnectionString, TStateConnectionString>
        where TEventStoreConnectionString : class
        where TStateConnectionString : class
    {
        public static Func<AdoNetTransactionUowProvider<TStateConnectionString>, LastSeen> LastSeenFunction { get; set; }
        public static Func<AdoNetConnectionUowProvider<TEventStoreConnectionString>, RecentNotifications> RecentNotificationsFunction { get; set; }
        public static Func<AdoNetTransactionUowProvider<TStateConnectionString>, RecordLastSeen> RecordLastSeenFunction { get; set; }

        public static CommitWork<AdoNetTransactionUowProvider<TStateConnectionString>> CommitState = 
            AdoNetTransactionUowProvider<TStateConnectionString>.CommitWork(ConnectionString.ByName);

        public static CommitWork<AdoNetConnectionUowProvider<TEventStoreConnectionString>> CommitStream = 
            AdoNetConnectionUowProvider<TEventStoreConnectionString>.CommitWork(ConnectionString.ByName);

        public static void Handler(IReadOnlyCollection<Subscription> subscriptions)
        {
            Functions.Handle
            (
                CommitState,
                CommitStream,
                LastSeenFunction,
                RecentNotificationsFunction,
                subscriptions.Select(x => x.NotificationContract),
                notifications => PostBox<AdoNetTransactionScopeUowProvider>.Post
                (
                    notifications.SelectMany(notification => 
                        new Event
                        {
                            EventId = new NoEventId(),
                            Notification = notification
                        }.SubscriberMessages(subscriptions))
                ),
                RecordLastSeenFunction
            );
        }
    }
}
