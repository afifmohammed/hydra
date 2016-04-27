using System;
using System.Configuration;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using InventoryStockManager.Domain;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

namespace InventoryStockManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri =
                new Uri("http://localhost:3579");

            foreach (var element in InventoryItemStockHandler.Subsriptions().PublisherBySubscription)
            {
                EventStore.PublishersBySubscription.Add(element.Key, element.Value);
            }

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.CommitEventStoreConnection =
                AdoNetTransaction<ApplicationStore>.CommitWork(ConnectionString.ByName);

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.CommitTransportConnection =
                AdoNetTransactionScope.Commit();

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: "EventStoreTransport",
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            EventStore<AdoNetTransaction<ApplicationStore>>.NotificationsByCorrelations =
                t => SqlQueries.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.PublisherVersionByPublisherDataContractCorrelations =
                t => SqlQueries.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.SaveNotificationsByPublisherAndVersion =
                t => SqlQueries.SaveNotificationsByPublisherAndVersion(t.Value);

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Enqueue = (endpoint, messages) =>
            {
                foreach (var subscriberMessage in messages)
                {
                    var message = new MailboxJsonMessage
                    {
                        NotificationContent = new JsonContent(subscriberMessage.Notification),
                        NotificationType = subscriberMessage.Notification.GetType().FullName,
                        Subscription = subscriberMessage.Subscription
                    };

                    BackgroundJob.Enqueue(() => new JsonMessageMailbox().Route(message));
                }
            };

            using (var host = new NancyHost(uri))
            {
                var svr = new BackgroundJobServer();
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();

                svr.Dispose();
            }
        }
    }

    public class MailboxJsonMessage
    {
        public Subscription Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public string NotificationType { get; set; }
    }

    public class JsonMessageMailbox
    {
        public void Route(MailboxJsonMessage message)
        {
            var subscriberMessage = new SubscriberMessage
            {
                Subscription = message.Subscription,
                Notification = (IDomainEvent) JsonConvert.DeserializeObject(message.NotificationContent.Value, Type.GetType(message.NotificationType))
            };

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Route(subscriberMessage);
        }
    }

    static class ConnectionString
    {
        public static Func<string, string> ByName = connectionStringName => ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
    }   
}