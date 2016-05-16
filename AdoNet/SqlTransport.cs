using System;
using System.Linq.Expressions;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;

namespace AdoNet
{
    public static class SqlTransport
    {
        public static void Initialize<TEventStoreName, TTransportStoreName>(Func<string, string> connectionString) where TEventStoreName : class
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: connectionString(typeof(TTransportStoreName).FriendlyName()),
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            JsonEventStoreMessageHandler<AdoNetTransaction<TEventStoreName>>.Post = messages => PostBox<AdoNetTransactionScope>.Post(messages);

            PostBox<AdoNetTransactionScope>.CommitTransportConnection = AdoNetTransactionScope.Commit();

            PostBox<AdoNetTransactionScope>.Enqueue = (endpoint, messages) =>
            {
                foreach (var subscriberMessage in messages)
                {
                    var message = new JsonMessage
                    {
                        NotificationContent = new JsonContent(subscriberMessage.Notification),
                        NotificationType = subscriberMessage.Notification.GetType(),
                        Subscription = new JsonContent(subscriberMessage.Subscription),
                        SubscriptionType = subscriberMessage.Subscription.GetType(),
                    };

                    BackgroundJob.Enqueue(() => SqlTransport.Handle<TEventStoreName>(message));
                }
            };
        }

        public static void Handle<TStoreName>(JsonMessage message) where TStoreName : class
        {
            JsonEventStoreMessageHandler<AdoNetTransaction<TStoreName>>.Handle(message);
        }
    }
}