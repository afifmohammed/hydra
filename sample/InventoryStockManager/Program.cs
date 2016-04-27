using System;
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

            Mailbox<AdoNetTransaction<ApplicationStore>, TransactionScope>.CommitEventStoreConnection = 
                AdoNetTransaction<ApplicationStore>.CommitWork();

            Mailbox<AdoNetTransaction<ApplicationStore>, TransactionScope>.CommitTransportConnection = work =>
            {
                using (var scope = new TransactionScope())
                {
                    work(scope);
                    scope.Complete();
                }
            };

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: "EventStoreTransport", 
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = false
                });

            Mailbox<AdoNetTransaction<ApplicationStore>, TransactionScope>.Enqueue = (endpoint, messages) =>
            {
                foreach (var subscriberMessage in messages)
                    BackgroundJob.Enqueue(
                        () => Mailbox<AdoNetTransaction<ApplicationStore>, TransactionScope>.Route(subscriberMessage));
            };

            using (var host = new NancyHost(uri))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseHangfireDashboard();
        }
    }

    class ApplicationStore { }    
}
