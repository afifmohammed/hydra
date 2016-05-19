using EventSourcing;

namespace AdoNet
{
    public static class SqlStoreConfiguration
    {
        public static EventStoreConfiguration ConfigurePublishers<TEventStoreConnectionStringName>(this EventStoreConfiguration config)
            where TEventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.NotificationsByCorrelationsFunction =
                t => SqlEventStore.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.PublisherVersionByPublisherDataContractCorrelationsFunction =
                t => SqlEventStore.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.SaveNotificationsByPublisherAndVersionAction =
                t => SqlEventStore.SaveNotificationsByPublisherAndVersion(t.Value);

            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.CommitEventStoreConnection =
                AdoNetTransaction<TEventStoreConnectionStringName>.CommitWork(ConnectionString.ByName);
            return config;
        }

        public static EventStoreConfiguration ConfigurePublishingNotifications<TEventStoreConnectionStringName>(this EventStoreConfiguration config)
            where TEventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.Post = PostBox<AdoNetTransactionScope>.Post;
            return config;
        }        
    }
}
