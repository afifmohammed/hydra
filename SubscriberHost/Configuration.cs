using System;
using System.Linq;
using Hydra.AdoNet;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;
using Hydra.Subscriptions;

namespace Hydra.SubscriberHost
{
    public static class SubscriberHostConfiguration
    {
        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigureSubscribers<TEventStoreConnectionStringName>(
            this EventStoreConfiguration<TEventStoreConnectionStringName> configuration,
            PublishersBySubscription subscribers)
            where TEventStoreConnectionStringName : class
        {
            configuration.ConfigureSubscriptions(subscribers.Select(x => x.Key));

            new RequestsRegistration<PublishersBySubscription>(() => subscribers)
                .Register<ConfiguredSubscribers, Subscriber>(
                    (input, provider) => EventStore<AdoNetTransactionProvider<TEventStoreConnectionStringName>>.Subscriber(provider), 
                    Return.List);

            return configuration;
        }

        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigureSubscribers<TEventStoreConnectionStringName, TExportProvider>(
            this EventStoreConfiguration<TEventStoreConnectionStringName> configuration,
            ProjectorsBySubscription<TExportProvider> subscribers)
            where TEventStoreConnectionStringName : class 
            where TExportProvider : IProvider
        {
            configuration.ConfigureSubscriptions(subscribers.Select(x => x.Key));

            new RequestsRegistration<ProjectorsBySubscription<TExportProvider>>(() => subscribers)
                .Register<ConfiguredSubscribers, Subscriber>(
                    (input, provider) => ExporterStore<AdoNetConnectionProvider<TEventStoreConnectionStringName>, TExportProvider>.Subscriber(provider), 
                    Return.List);

            return configuration;
        }

        public static IDisposable StartHost<TEventStoreConnectionStringName, THangfireDatabaseConnectionStringName>(
            this EventStoreConfiguration<TEventStoreConnectionStringName> configuration)
            where TEventStoreConnectionStringName : class where THangfireDatabaseConnectionStringName : class
        {
            return new HangfireSubscriberHost<TEventStoreConnectionStringName, THangfireDatabaseConnectionStringName>
            (
                () => Request<Subscriber>.By(new ConfiguredSubscribers())
            );
        }
    }
}
