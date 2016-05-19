using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using Polling;

namespace AdoNet
{
    public static class Polling<TEventStoreConnectionString, TStateConnectionString>
        where TEventStoreConnectionString : class
        where TStateConnectionString : class
    {
        public static Func<AdoNetTransaction<TStateConnectionString>, LastSeen> LastSeenFunction { get; set; }
        public static Func<AdoNetConnection<TEventStoreConnectionString>, RecentNotifications> RecentNotificationsFunction { get; set; }
        public static Func<AdoNetTransaction<TStateConnectionString>, RecordLastSeen> RecordLastSeenFunction { get; set; }

        public static CommitWork<AdoNetTransaction<TStateConnectionString>> CommitState = 
            AdoNetTransaction<TStateConnectionString>.CommitWork(ConnectionString.ByName);

        public static CommitWork<AdoNetConnection<TEventStoreConnectionString>> CommitStream = 
            AdoNetConnection<TEventStoreConnectionString>.CommitWork(ConnectionString.ByName);

        public static void Handler(IEnumerable<Subscription> subscriptions)
        {
            Polling.Functions.Handle
            (
                CommitState,
                CommitStream,
                LastSeenFunction,
                RecentNotificationsFunction,
                subscriptions.Select(x => x.NotificationContract),
                notifications => PostBox<AdoNetTransactionScope>.Post(
                    PostBox<AdoNetTransactionScope>.SubscriberMessages(
                        notifications, 
                        new SubscriberMessagesByNotification[] 
                        {
                            notification => subscriptions
                                .Where(subscription => subscription.NotificationContract.Equals(new TypeContract(notification)))
                                .Select(subscription => new SubscriberMessage { Notification = notification, Subscription = subscription })
                        }.ToList())),
                RecordLastSeenFunction
            );
        }
    }
}
