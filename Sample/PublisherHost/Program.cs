using System;
using System.Linq;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using Nancy;
using Nancy.Hosting.Self;
using RetailDomain.Inventory;
using RetailDomain.Refunds;

namespace PublisherHost
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlEventStore.Initialize<ApplicationStore>(ConnectionString.ByName, handler => BackgroundJob.Enqueue(handler));
            
            foreach (var element in new PublishersBySubscription()
                .Union(InventoryItemStockHandler.Subsriptions().PublisherBySubscription)
                .Union(RefundProductOrderHandler.Subscriptions().PublisherBySubscription))
            {
                EventStore.PublishersBySubscription.Add(element.Key, element.Value);
            }

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: "EventStoreTransport",
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            var uri = new Uri("http://localhost:3579");

            using (new BackgroundJobServer())
            using (var host = new NancyHost(
                uri, 
                new DefaultNancyBootstrapper(), 
                new HostConfiguration { UrlReservations = new UrlReservations() { CreateAutomatically = true } }))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }
}
