using System;
using System.Collections.Generic;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace SerializedInvocation
{
    public static class HangfireConfiguration
    {
        public static EventStoreConfiguration ConfigureTransport<THangfireConnectionStringName>(
            this EventStoreConfiguration config)
            where THangfireConnectionStringName : class
        {
            Initialize<THangfireConnectionStringName>();

            PostBox<AdoNetTransactionScope>.CommitTransportConnection = AdoNetTransactionScope.Commit();

            PostBox<AdoNetTransactionScope>.Enqueue = Enqueue;

            return config;
        }

        static void Initialize<THangfireConnectionStringName>()
            where THangfireConnectionStringName : class
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: ConnectionString.ByName(typeof(THangfireConnectionStringName).FriendlyName()),
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });
        }

        static void Enqueue(AdoNetTransactionScope endpoint, IEnumerable<SubscriberMessage> messages)
        {
            foreach (var subscriberMessage in messages)
            {
                var message = new JsonMessage(subscriberMessage);
                BackgroundJob.Enqueue(() => JsonMessageHandler.Handle(message));
            }
        }        
    }
}
