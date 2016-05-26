using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;

namespace Hydra.SubscriberHost
{
    public static class SubscriberHostConfiguration
    {
        public static EventStoreConfiguration<TEventStoreProvider> ConfigureSubscribers<TEventStoreProvider>(
            this EventStoreConfiguration<TEventStoreProvider> configuration,
            PublishersBySubscription subscribers)
            where TEventStoreProvider : class, IProvider
        {
            new RequestsRegistration<PublishersBySubscription>(() => subscribers)
                .Register<ConfiguredSubscriber, Subscriber>((input, provider) => EventStore<TEventStoreProvider>.Subscriber(provider), Return.List);
            return configuration;
        }

        public static EventStoreConfiguration<TEventStoreProvider> ConfigureSubscribers<TEventStoreProvider, TExportProvider>(
            this EventStoreConfiguration<TEventStoreProvider> configuration,
            ExportersBySubscription<TExportProvider> subscribers)
            where TEventStoreProvider : class, IProvider 
            where TExportProvider : IProvider
        {
            new RequestsRegistration<ExportersBySubscription<TExportProvider>>(() => subscribers)
                .Register<ConfiguredSubscriber, Subscriber>((input, provider) => ExporterStore<TEventStoreProvider, TExportProvider>.Subscriber(provider), Return.List);

            return configuration;
        }
    }
}
