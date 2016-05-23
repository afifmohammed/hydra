using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AdoNet;
using EventSourcing;
using RequestPipeline;

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

    class ApplicationRequestPipeline
    {
        public static IEnumerable<Subscription> Subscriptions { get; set; }

        public static Response<Unit> Dispatch<TCommand>(TCommand command) where TCommand : IRequest<Unit>, ICorrelated
        {
            return RequestPipeline<AdoNetTransactionScopeProvider>.Dispatch<TCommand>(Subscriptions)(command);
        }
    }
}
