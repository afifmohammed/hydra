using System;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using Nancy;
using Nancy.Hosting.Self;
using RetailDomain.Inventory;

namespace WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var element in InventoryItemStockHandler.Subsriptions().PublisherBySubscription)
            {
                EventStore.PublishersBySubscription.Add(element.Key, element.Value);
            }

            SqlEventStore.Initialize<ApplicationStore>(ConnectionString.ByName, message => BackgroundJob.Enqueue(message));

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: "EventStoreTransport",
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };

            var uri = new Uri("http://localhost:3785");
            using (var host = new NancyHost(uri, new DefaultNancyBootstrapper(), hostConfigs))
            using (new BackgroundJobServer())
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }
}