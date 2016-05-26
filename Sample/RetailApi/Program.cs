using System;
using Hydra.Configuration;
using Hydra.SerializedInvocation;
using Hydra.Subscriptions;
using Nancy;
using Nancy.Hosting.Self;
using RetailDomain.Inventory;
using RetailDomain.Refunds;

namespace WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            new EventStoreConfiguration()
                .ConfigureTransport<EventStoreTransportConnectionString>()
                .ConfigureSubscriptions(
                    InventoryItemStockHandler.Subscriptions().PublisherBySubscription,
                    RefundProductOrderHandler.Subscriptions().PublisherBySubscription);
                 

            var uri = new Uri("http://localhost:3785");

            using (var host = new NancyHost(
                uri,
                new DefaultNancyBootstrapper(),
                new HostConfiguration { UrlReservations = new UrlReservations() { CreateAutomatically = true } }))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the Api Host.");
                Console.ReadLine();
            }
        }
    }
}