﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.SqlServer;
using Hydra.AdoNet;
using Hydra.Configuration;
using Hydra.Core;
using Hydra.Requests;
using Hydra.Subscriptions;

namespace Hydra.SerializedInvocation
{
    public static class HangfireConfiguration
    {
        public static EventStoreConfiguration ConfigureMessageHandler(this EventStoreConfiguration config)
        {
            JsonMessageHandler.HandleInstance =
                m =>
                {
                    foreach (var subscriber in Request<Subscriber>.By(new ConfiguredSubscribers()))
                        subscriber(m);
                };
            return config;
        }

        public static EventStoreConfiguration ConfigureTransport<THangfireConnectionStringName>(
            this EventStoreConfiguration config)
            where THangfireConnectionStringName : class
        {
            Initialize<THangfireConnectionStringName>();

            PostBox<AdoNetTransactionScopeUowProvider>.CommitWork = AdoNetTransactionScopeUowProvider.Commit();

            PostBox<AdoNetTransactionScopeUowProvider>.Enqueue = Enqueue;

            return config;
        }

        public static BackgroundJobServerOptions QueuePerSubscriber(this BackgroundJobServerOptions options)
        {
            options.Queues =
                Request<Subscription>.By(new RegisteredSubscriptions())
                    .Select(x => x.SubscriberDataContract.Value.ToLower())
                    .ToArray();

            options.WorkerCount = 1;

            return options;
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

        static void Enqueue(AdoNetTransactionScopeUowProvider uowProvider, IEnumerable<SubscriberMessage> messages)
        {
            foreach (var subscriberMessage in messages)
            {
                var message = new JsonMessage(subscriberMessage);
                BackgroundJob.Enqueue(() => JsonMessageHandler.Handle(message));
            }
        }
    }
}
