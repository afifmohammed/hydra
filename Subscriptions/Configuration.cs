using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;

namespace Hydra.Subscriptions
{
    public static class SubscriptionConfiguration
    {
        public static EventStoreConfiguration ConfigureSubscriptions(this EventStoreConfiguration configuration, params IEnumerable<Subscription>[] subscriptions)
        {
            configuration.ConfigureSubscriptions(subscriptions
                .SelectMany(x => x)
                .ToArray());

            return configuration;
        }

        public static EventStoreConfiguration ConfigureSubscriptions(this EventStoreConfiguration configuration, params Subscription[] subscriptions)
        {
            new RequestsRegistration<Disposable<List<Subscription>>>(() => new Disposable<List<Subscription>>(new List<Subscription>(subscriptions)))
                .Register<RegisteredSubscriptions, Subscription>((q, list) => list.Value);

            return configuration;
        }

        class Disposable<T> : Wrapper<T>, IDisposable
        {
            public Disposable(T instance)
            {
                Value = instance;
            }

            public void Dispose()
            { }

            public T Value { get; }
        }
    }
}
