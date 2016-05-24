using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using AdoNet;
using EventSourcing;
using RequestPipeline;
using Requests;

namespace Subscriptions
{
    public class SubscriberPipeline
    {
        public static Func<IEnumerable<Subscription>> GetSubscriptions = () => Request<Subscription>.By(new AvailableSubscriptions());

        public static Response<Unit> Dispatch<TCommand>(TCommand command) where TCommand : IRequest<Unit>, ICorrelated
        {
            var subscriptions = new Lazy<IReadOnlyCollection<Subscription>>(() => new List<Subscription>(GetSubscriptions()).AsReadOnly());

            return RequestPipeline<AdoNetTransactionScopeProvider>.Dispatch<TCommand>(() => subscriptions.Value)(command);
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

        class Disposable<T> : Unit<T>, IDisposable
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
