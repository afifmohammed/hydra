using System;
using System.Linq;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using Nancy.Hosting.Self;
using ValuationService.Domain;

namespace ValuationService
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://localhost:3578");

            var subscruptions = new[]
            {
                UpdateCustomerHandler.Subscriptions(),
                RequestValuationHandler.Subscriptions(),
                ProcessValuationHandler.Subscriptions()
            };
            
            foreach (var element in subscruptions.SelectMany(s=>s.PublisherBySubscription))
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

            using (var host = new NancyHost(uri))
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
