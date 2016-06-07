using System;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;

namespace Hydra.SubscriberHost
{
    public static class SubscriberHostConfiguration
    {
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
