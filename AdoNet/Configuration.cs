using EventSourcing;

namespace AdoNet
{
    public static class SqlStoreConfiguration
    {
        internal static EventStoreConfiguration ConfigurePublishers<EventStoreConnectionStringName>(this EventStoreConfiguration config)
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

        internal static EventStoreConfiguration ConfigurePublishingNotifications<EventStoreConnectionStringName>(this EventStoreConfiguration config)
            where EventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.Post = PostBox<AdoNetTransactionScope>.Post;
            return config;
        }
    }

}
