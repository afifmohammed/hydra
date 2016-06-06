using System;
using System.Linq;
using Hydra.AdoNet;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;
using Hydra.Subscribers;
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
                    (input, provider) => EventStore<AdoNetTransactionUowProvider<TEventStoreConnectionStringName>>.Subscriber(provider), 
                    Return.List);

            return configuration;
        }

        public static EventStoreConfiguration<TEventStoreConnectionStringName> ConfigureSubscribers<TEventStoreConnectionStringName, TProjectionProvider>(
            this EventStoreConfiguration<TEventStoreConnectionStringName> configuration,
            ProjectorsBySubscription<TProjectionProvider> subscribers)
            where TEventStoreConnectionStringName : class 
            where TProjectionProvider : IUowProvider
        {
            configuration.ConfigureSubscriptions(subscribers.Select(x => x.Key));

            new RequestsRegistration<ProjectorsBySubscription<TProjectionProvider>>(() => subscribers)
                .Register<ConfiguredSubscribers, Subscriber>(
                    (input, provider) => ProjectorStore<AdoNetConnectionUowProvider<TEventStoreConnectionStringName>, TProjectionProvider>.Subscriber(provider), 
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
