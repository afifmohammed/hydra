using System;
using System.Configuration;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using InventoryStockManager.Domain;
using Nancy.Hosting.Self;

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
                t => SqlEventStore.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.PublisherVersionByPublisherDataContractCorrelations =
                t => SqlEventStore.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.SaveNotificationsByPublisherAndVersion =
                t => SqlEventStore.SaveNotificationsByPublisherAndVersion(t.Value);

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Enqueue = (endpoint, messages) =>
            {
                foreach (var subscriberMessage in messages)
                {
                    var message = new JsonMailboxMessage
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
}