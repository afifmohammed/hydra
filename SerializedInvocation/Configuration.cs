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
        public static EventStoreConfiguration<TEventSource, TTransport> ConfigureEventStoreWithTransportConnection<TEventSource, TTransport>(
            this EventStoreConfiguration config)
            where TEventSource : class
            where TTransport : class
        {
            return new EventStoreConfiguration<TEventSource, TTransport>();
        }

        public static EventStoreConfiguration<EventStoreConnectionStringName, HangfireConnectionStringName> ConfigureTransport<EventStoreConnectionStringName, HangfireConnectionStringName>(
            this EventStoreConfiguration<EventStoreConnectionStringName, HangfireConnectionStringName> config)
            where EventStoreConnectionStringName : class
            where HangfireConnectionStringName : class
        {
            Initialize<EventStoreConnectionStringName, HangfireConnectionStringName>();

            PostBox<AdoNetTransactionScope>.CommitTransportConnection = AdoNetTransactionScope.Commit();

            PostBox<AdoNetTransactionScope>.Enqueue = Enqueue;

            return config;
        }

        static void Initialize<EventStoreConnectionStringName, HangfireConnectionStringName>()
            where HangfireConnectionStringName : class
            where EventStoreConnectionStringName : class
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: ConnectionString.ByName(typeof(HangfireConnectionStringName).FriendlyName()),
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            JsonMessageHandler.HandleInstance = message =>
            {
                var subscriberMessage = new SubscriberMessage();

                subscriberMessage.Subscription = (Subscription)JsonConvert.DeserializeObject(
                    message.Subscription.Value,
                    message.SubscriptionType);

                subscriberMessage.Notification = (IDomainEvent)JsonConvert.DeserializeObject(
                    message.NotificationContent.Value,
                    message.NotificationType);

                EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.Handle(subscriberMessage);
            };
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
