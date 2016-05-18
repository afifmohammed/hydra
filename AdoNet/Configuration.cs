using EventSourcing;

namespace AdoNet
{
    public static class SqlStoreConfiguration
    {
        public static EventStoreConfiguration ConfigurePublishers<EventStoreConnectionStringName>(this EventStoreConfiguration config)
            where EventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.NotificationsByCorrelations =
                t => SqlEventStore.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.PublisherVersionByPublisherDataContractCorrelations =
                t => SqlEventStore.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.SaveNotificationsByPublisherAndVersion =
                t => SqlEventStore.SaveNotificationsByPublisherAndVersion(t.Value);

            EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.CommitEventStoreConnection =
                AdoNetTransaction<EventStoreConnectionStringName>.CommitWork(ConnectionString.ByName);
            return config;
        }

        public static EventStoreConfiguration ConfigurePublishingNotifications<EventStoreConnectionStringName>(this EventStoreConfiguration config)
            where EventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.Post = PostBox<AdoNetTransactionScope>.Post;
            return config;
        }

        public static EventStoreConfiguration ConfigureTransport<HangfireConnectionStringName, EventStoreConnectionStringName>(this EventStoreConfiguration config)
            where EventStoreConnectionStringName : class
            where HangfireConnectionStringName : class
        {
            Hangfire.Initialize<HangfireConnectionStringName, EventStoreConnectionStringName>();

            PostBox<AdoNetTransactionScope>.CommitTransportConnection = AdoNetTransactionScope.Commit();

            PostBox<AdoNetTransactionScope>.Enqueue = Hangfire.Enqueue;

            return config;
        }
    }
}
