using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using Requests;

namespace SubscriberHost
{
    public static class SubscriberHostConfiguration
    {
        class PublisherConfigurator : IDisposable
        {
            public PublisherConfigurator(PublishersBySubscription publishers, Func<IEnumerable<Subscription>> getSubscriptions)
            {
                Publishers = publishers;
                GetSubscriptions = getSubscriptions;
            }

            public readonly PublishersBySubscription Publishers;
            public readonly Func<IEnumerable<Subscription>> GetSubscriptions;

            public void Dispose()
            {}
        }

        public static EventStoreConfiguration<TEventStoreProvider> ConfigureSubscribers<TEventStoreProvider>(
            this EventStoreConfiguration<TEventStoreProvider> configuration,
            PublishersBySubscription subscribers)
            where TEventStoreProvider : class, IProvider
        {
            return configuration.ConfigureSubscribers(subscribers, Enumerable.Empty<Subscription>);
        }

        public static EventStoreConfiguration<TEventStoreProvider> ConfigureSubscribers<TEventStoreProvider>(
            this EventStoreConfiguration<TEventStoreProvider> configuration,
            PublishersBySubscription subscribers,
            Func<IEnumerable<Subscription>> getSubscriptions)
            where TEventStoreProvider : class, IProvider
        {
            new RequestsRegistration<PublisherConfigurator>(() => new PublisherConfigurator(subscribers, getSubscriptions))
                .Register<ConfiguredSubscriber, Subscriber>((input, provider) => EventStore<TEventStoreProvider>.Subscriber(provider.Publishers, provider.GetSubscriptions), Return.List);
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
