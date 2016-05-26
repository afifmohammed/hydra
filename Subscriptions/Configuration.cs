using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Hydra.AdoNet;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Core.FluentInterfaces;
using Hydra.RequestPipeline;
using Hydra.Requests;

namespace Hydra.Subscriptions
{
    public class SubscriberPipeline
    {
        public static Func<IEnumerable<Subscription>> GetSubscriptions = () => Request<Subscription>.By(new AvailableSubscriptions());

        public static Response<Unit> Dispatch<TRequest>(TRequest command) where TRequest : IRequest<Unit>, ICorrelated
        {
            var subscriptions = new Lazy<IReadOnlyCollection<Subscription>>(() => new List<Subscription>(GetSubscriptions()).AsReadOnly());

            return RequestPipeline<AdoNetTransactionScopeProvider>.Place<TRequest>(() => subscriptions.Value)(command);
        }
    }

    public static class RequestsConfiguration
    {
        public static EventStoreConfiguration<TSubscriptionStoreConnectionStringName> ConfigureSubscriptions<TSubscriptionStoreConnectionStringName>(
            this EventStoreConfiguration<TSubscriptionStoreConnectionStringName> configuration,
            Func<IDbConnection, IEnumerable<Subscription>> subscriptionQuery)
            where TSubscriptionStoreConnectionStringName : class
        {
            new RequestsRegistration<IDbConnection>(() => new SqlConnection(ConnectionString.ByName(typeof(TSubscriptionStoreConnectionStringName).FriendlyName())).With(x => x.Open()))
                .Register<AvailableSubscriptions, Subscription>((q, connection) => subscriptionQuery(connection));

            return configuration;
        }

        public static EventStoreConfiguration ConfigureSubscriptions(this EventStoreConfiguration configuration, params PublisherSubscriptions[] subscriptions)
        {
            configuration.ConfigureSubscriptions(subscriptions
                .SelectMany(x => x.PublisherBySubscription)
                .Select(x => x.Key)
                .ToArray());

            return configuration;
        }

        public static EventStoreConfiguration ConfigureSubscriptions(this EventStoreConfiguration configuration, params Subscription[] subscriptions)
        {
            new RequestsRegistration<Disposable<List<Subscription>>>(() => new Disposable<List<Subscription>>(new List<Subscription>(subscriptions)))
                .Register<AvailableSubscriptions, Subscription>((q, list) => list.Value);

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
