using System;
using System.Collections.Generic;
using Hangfire;
using Hydra.AdoNet;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.SerializedInvocation;

namespace Hydra.SubscriberHost
{
    class HangfireSubscriberHost<TEventStoreConnectionStringName, THangfireDatabaseConnectionStringName> : IDisposable 
        where THangfireDatabaseConnectionStringName : class 
        where TEventStoreConnectionStringName : class
    {
        readonly BackgroundJobServer _server;

        public HangfireSubscriberHost(Func<IEnumerable<Subscriber>> getSubscribers)
        {
            new EventStoreConfiguration<TEventStoreConnectionStringName>()
                .ConfigureEventStoreConnection<TEventStoreConnectionStringName>()
                .ConfigurePublishers()
                .ConfigureTransport<THangfireDatabaseConnectionStringName>();

            var options = new BackgroundJobServerOptions().With(x => x.QueuePerSubscriber());
            _server = new BackgroundJobServer(options);

            JsonMessageHandler.HandleInstance = message =>
            {
                foreach (var subscriber in getSubscribers())
                {
                    subscriber(message);
                }
            };
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}