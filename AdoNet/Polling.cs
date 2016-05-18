using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using Polling;

namespace AdoNet
{
    public static class Polling<EventStoreConnectionString, StateConnectionString>
        where EventStoreConnectionString : class
        where StateConnectionString : class
    {
        public static Func<AdoNetTransaction<StateConnectionString>, LastSeen> LastSeenFunction { get; set; }
        public static Func<AdoNetConnection<EventStoreConnectionString>, RecentNotifications> RecentNotificationsFunction { get; set; }
        public static Func<AdoNetTransaction<StateConnectionString>, RecordLastSeen> RecordLastSeenFunction { get; set; }

        public static CommitWork<AdoNetTransaction<StateConnectionString>> CommitState = 
            AdoNetTransaction<StateConnectionString>.CommitWork(ConnectionString.ByName);

        public static CommitWork<AdoNetConnection<EventStoreConnectionString>> CommitStream = 
            AdoNetConnection<EventStoreConnectionString>.CommitWork(ConnectionString.ByName);

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
