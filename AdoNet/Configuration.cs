using EventSourcing;

namespace AdoNet
{
    public static class SqlStoreConfiguration
    {
        public static EventStoreConfiguration<EventStoreConnectionStringName> ConfigurePublishers<EventStoreConnectionStringName>(this EventStoreConfiguration<EventStoreConnectionStringName> config)
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

        public static EventStoreConfiguration<EventStoreConnectionStringName> ConfigurePublishingNotifications<EventStoreConnectionStringName>(this EventStoreConfiguration<EventStoreConnectionStringName> config)
            where EventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.Post = PostBox<AdoNetTransactionScope>.Post;
            return config;
        }

        public static EventStoreConfiguration<TEventStore> ConfigureEventStoreConnection<TEventStore>(this EventStoreConfiguration config) where TEventStore : class
        {
            return new EventStoreConfiguration<TEventStore>();
        }
    }
}
