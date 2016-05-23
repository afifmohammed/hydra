using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using AdoNet;
using EventSourcing;
using RequestPipeline;
using Requests;

namespace WebApi
{
    /// <summary>
    /// This class is used as place holder to specify that the name of the connection string is <see cref="EventStoreConnectionString"/>
    /// </summary>
    class EventStoreConnectionString
    { }

    /// <summary>
    /// This class is used as place holder to specify that the name of the connection string is <see cref="EventStoreTransportConnectionString"/>
    /// </summary>
    class EventStoreTransportConnectionString { }

    class EverySubscription : IRequest<Subscription> { }

    class ApplicationRequestPipeline
    {
        public static Func<IEnumerable<Subscription>> GetSubscriptions = () => Request<Subscription>.By(new EverySubscription());

        public static Response<Unit> Dispatch<TCommand>(TCommand command) where TCommand : IRequest<Unit>, ICorrelated
        {
            var subscriptions = new Lazy<IReadOnlyCollection<Subscription>>(() => new List<Subscription>(GetSubscriptions()).AsReadOnly());

            return RequestPipeline<AdoNetTransactionScopeProvider>.Dispatch<TCommand>(() => subscriptions.Value)(command);
        }
    }
}
