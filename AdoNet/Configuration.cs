using EventSourcing;

namespace AdoNet
{
    public static class SqlStoreConfiguration
    {
        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigurePublishers<TEventStoreConnectionStringName>(this EventStoreConfiguration<TEventStoreConnectionStringName> config)
            where TEventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.NotificationsByCorrelationsFunction =
                t => SqlEventStore.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.PublisherVersionByCorrelationsFunction =
                t => SqlEventStore.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.SaveNotificationsByPublisherAndVersionAction =
                t => SqlEventStore.SaveNotificationsByPublisherAndVersion(t.Value);

            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.CommitEventStoreConnection =
                AdoNetTransaction<TEventStoreConnectionStringName>.CommitWork(ConnectionString.ByName);
            return config;
        }

        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigurePublishingNotifications<TEventStoreConnectionStringName>(this EventStoreConfiguration<TEventStoreConnectionStringName> config)
            where TEventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<TEventStoreConnectionStringName>>.Post = PostBox<AdoNetTransactionScope>.Post;
            return config;
        }

        public static EventStoreConfiguration<TEventStore> ConfigureEventStoreConnection<TEventStore>(this EventStoreConfiguration config) where TEventStore : class
        {
            return new EventStoreConfiguration<TEventStore>();
        }
    }
}
