﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;
using Hydra.PublishOnPull;
using Hydra.Subscriptions;

namespace Hydra.AdoNet
{
    public static class PublishOnPullFromSqlEventStore<TEventStoreConnectionString, TStateConnectionString>
        where TEventStoreConnectionString : class
        where TStateConnectionString : class
    {
        public static Func<AdoNetTransactionProvider<TStateConnectionString>, LastSeen> LastSeenFunction { get; set; }
        public static Func<AdoNetConnectionProvider<TEventStoreConnectionString>, RecentNotifications> RecentNotificationsFunction { get; set; }
        public static Func<AdoNetTransactionProvider<TStateConnectionString>, RecordLastSeen> RecordLastSeenFunction { get; set; }

        public static CommitWork<AdoNetTransactionProvider<TStateConnectionString>> CommitState = 
            AdoNetTransactionProvider<TStateConnectionString>.CommitWork(ConnectionString.ByName);

        public static CommitWork<AdoNetConnectionProvider<TEventStoreConnectionString>> CommitStream = 
            AdoNetConnectionProvider<TEventStoreConnectionString>.CommitWork(ConnectionString.ByName);

        public static void Handler(IReadOnlyCollection<Subscription> subscriptions)
        {
            PublishOnPull.Functions.Handle
            (
                CommitState,
                CommitStream,
                LastSeenFunction,
                RecentNotificationsFunction,
                subscriptions.Select(x => x.NotificationContract),
                notifications => PostBox<AdoNetTransactionScopeProvider>.Post(notifications.SelectMany(notification => new Event {EventId = new NoEventId(), Notification = notification}.SubscriberMessages(subscriptions))),
                RecordLastSeenFunction
            );
        }
    }
}
