using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Transactions;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using InventoryStockManager.Domain;
using Nancy.Hosting.Self;
using Owin;

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

            EventStore<AdoNetTransaction<ApplicationStore>>.NotificationsByCorrelations =
                t => SqlQueries.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.PublisherVersionByPublisherDataContractCorrelations =
                t => SqlQueries.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.SaveNotificationsByPublisherAndVersion =
                t => SqlQueries.SaveNotificationsByPublisherAndVersion(t.Value);

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


            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Enqueue = (endpoint, messages) =>
            {
                foreach (var subscriberMessage in messages)
                {
                    Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Route(subscriberMessage);

                    //BackgroundJob.Enqueue(() => Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Route(subscriberMessage));
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
    
    class ApplicationStore
    {}

    static class ConnectionString
    {
        public static string ByName(string connectionStringName)
        {
            return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        }
    }   
}
