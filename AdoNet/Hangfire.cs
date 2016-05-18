using System;
using System.Collections.Generic;
using System.Configuration;
using EventSourcing;
using Hangfire;
using Hangfire.Common;
using Hangfire.SqlServer;
using Hangfire.States;
using Newtonsoft.Json;

namespace AdoNet
{
    public static class JsonMessageHandler
    {
        [UseQueueFromParameter(0)]
        public static void Handle(JsonMessage message)
        {
            HandleInstance(message);
        }

        public static Action<JsonMessage> HandleInstance = m => { };
    }

    public static class Hangfire
    {
        public static void Initialize<HangfireConnectionStringName, EventStoreConnectionStringName>()
            where HangfireConnectionStringName : class
            where EventStoreConnectionStringName : class
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: ConnectionString(typeof(HangfireConnectionStringName).FriendlyName()),
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            JsonMessageHandler.HandleInstance = message =>
            {
                var subscriberMessage = new SubscriberMessage();

                subscriberMessage.Subscription = (Subscription)JsonConvert.DeserializeObject(message.Subscription.Value, message.SubscriptionType);
                subscriberMessage.Notification = (IDomainEvent)JsonConvert.DeserializeObject(message.NotificationContent.Value, message.NotificationType);

                EventStore<AdoNetTransaction<EventStoreConnectionStringName>>.Handle(subscriberMessage);
            };
        }

        public static void Enqueue(AdoNetTransactionScope endpoint, IEnumerable<SubscriberMessage> messages)
        {
            foreach (var subscriberMessage in messages)
            {
                var message = new JsonMessage(subscriberMessage);
                BackgroundJob.Enqueue(() => JsonMessageHandler.Handle(message));
            }
        }    
        
        public static Func<string, string> ConnectionString = name => ConfigurationManager.ConnectionStrings[name].ConnectionString;
    }

    public class JsonMessage
    {
        public JsonMessage()
        {
            HandlerAddress = "default";
        }

        public JsonMessage(SubscriberMessage subscriberMessage)
        {
            NotificationContent = new JsonContent(subscriberMessage.Notification);
            NotificationType = subscriberMessage.Notification.GetType();
            Subscription = new JsonContent(subscriberMessage.Subscription);
            SubscriptionType = subscriberMessage.Subscription.GetType();
            HandlerAddress = subscriberMessage.Subscription.SubscriberDataContract.Value;
        }

        public string HandlerAddress { get; set; }
        public JsonContent Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public Type NotificationType { get; set; }
        public Type SubscriptionType { get; set; }
    }

    public class UseQueueFromParameterAttribute : JobFilterAttribute, IElectStateFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueAttribute"/> class
        /// using the specified queue name.
        /// </summary>
        public UseQueueFromParameterAttribute(int parameterIndex)
        {
            this.ParameterIndex = parameterIndex;
        }

        public int ParameterIndex { get; private set; }

        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;

            if (enqueuedState == null) return;

            var parameter = context.BackgroundJob.Job.Args[ParameterIndex] as JsonMessage;

            if (ReferenceEquals(parameter, null)) return;
            enqueuedState.Queue = parameter.HandlerAddress.ToLower();
        }
    }
}