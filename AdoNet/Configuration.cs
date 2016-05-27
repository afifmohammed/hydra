using System.Linq;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;

namespace Hydra.AdoNet
{
    public static class SqlStoreConfiguration
    {
        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigurePublishers<TEventStoreConnectionStringName>(
            this EventStoreConfiguration<TEventStoreConnectionStringName> config)
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

        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigurePushNotifications<TEventStoreConnectionStringName>(
            this EventStoreConfiguration<TEventStoreConnectionStringName> config)
            where TEventStoreConnectionStringName : class
        {
            EventStore<AdoNetTransactionProvider<TEventStoreConnectionStringName>>.Publish = domainEvents =>
            {
                PostBox<AdoNetTransactionScopeProvider>.Drop
                    (() => Request<Subscription>.By(new AvailableSubscriptions()))
                    (domainEvents.Select(x => new Event {Notification = x, EventId = new NoEventId()}));
            };

            return config;
        }

        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigureEventStoreConnection<TEventStoreConnectionStringName>(
            this EventStoreConfiguration config) 
            where TEventStoreConnectionStringName : class
        {
            return new EventStoreConfiguration<TEventStoreConnectionStringName>();
        }
    }
}
