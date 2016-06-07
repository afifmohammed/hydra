using System.Linq;
using Hydra.AdoNet;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;
using Hydra.Subscriptions;

namespace Hydra.Subscribers
{
    public static class SubscriberConfiguration
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

        public static EventStoreConfiguration<TEventStoreUowProvider> ConfigureSubscribers<TEventStoreUowProvider, TProjectionProvider>(
            this EventStoreConfiguration<TEventStoreUowProvider> configuration,
            ProjectorsBySubscription<TProjectionProvider> subscribers)
            where TEventStoreUowProvider : class, IUowProvider
            where TProjectionProvider : IUowProvider
        {
            configuration.ConfigureSubscriptions(subscribers.Select(x => x.Key));

            new RequestsRegistration<ProjectorsBySubscription<TProjectionProvider>>(() => subscribers)
                .Register<ConfiguredSubscribers, Subscriber>(
                    (input, provider) => ProjectorStore<TEventStoreUowProvider, TProjectionProvider>.Subscriber(provider),
                    Return.List);

            return configuration;
        }
    }
}
