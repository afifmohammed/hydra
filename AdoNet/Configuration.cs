using EventSourcing;

namespace AdoNet
{
    public static class SqlStoreConfiguration
    {
        public static EventStoreConfiguration ConfigurePublishers<EventStoreConnectionStringName>(this EventStoreConfiguration config)
            where EventStoreConnectionStringName : class
        {
            SqlEventStore.ConfigurePublishers<EventStoreConnectionStringName>(ConnectionString.ByName);
            return config;
        }

        public static EventStoreConfiguration ConfigurePublishingNotifications<EventStoreConnectionStringName>(this EventStoreConfiguration config)
            where EventStoreConnectionStringName : class
        {
            SqlEventStore.ConfigurePublishingNotification<EventStoreConnectionStringName>();
            return config;
        }

        public static EventStoreConfiguration ConfigureTransport<HangfireConnectionStringName, EventStoreConnectionStringName>(this EventStoreConfiguration config)
            where EventStoreConnectionStringName : class
            where HangfireConnectionStringName : class
        {
            SqlTransport.Initialize<EventStoreConnectionStringName, HangfireConnectionStringName>(ConnectionString.ByName);
            return config;
        }
    }
}
