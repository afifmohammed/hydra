using EventSourcing;

namespace AdoNet
{
    public static class SqlStoreConfiguration
    {
        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigurePublishers<TEventStoreConnectionStringName>(this EventStoreConfiguration<TEventStoreConnectionStringName> config)
            where TEventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransactionProvider<TEventStoreConnectionStringName>>.NotificationsByCorrelationsFunction =
                t => SqlEventStore.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransactionProvider<TEventStoreConnectionStringName>>.PublisherVersionByCorrelationsFunction =
                t => SqlEventStore.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransactionProvider<TEventStoreConnectionStringName>>.SaveNotificationsByPublisherAndVersionAction =
                t => SqlEventStore.SaveNotificationsByPublisherAndVersion(t.Value);

            EventStore<AdoNetTransactionProvider<TEventStoreConnectionStringName>>.CommitEventStoreWork =
                AdoNetTransactionProvider<TEventStoreConnectionStringName>.CommitWork(ConnectionString.ByName);
            return config;
        }

        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigurePublishingNotifications<TEventStoreConnectionStringName>(this EventStoreConfiguration<TEventStoreConnectionStringName> config)
            where TEventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransactionProvider<TEventStoreConnectionStringName>>.Post = PostBox<AdoNetTransactionScopeProvider>.Post;
            return config;
        }

        public static EventStoreConfiguration<TEventStore> ConfigureEventStoreConnection<TEventStore>(this EventStoreConfiguration config) where TEventStore : class
        {
            return new EventStoreConfiguration<TEventStore>();
        }
    }
}
