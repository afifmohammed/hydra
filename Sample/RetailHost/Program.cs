using System;
using Hydra.Configuration;
using Hydra.SubscriberHost;

namespace RetailHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new EventStoreConfiguration<EventStoreConnectionString>()
                .ConfigureSubscribers(RetailDomain.Inventory.InventoryItemStockHandler.Subscriptions().PublisherBySubscription)
                .StartHost<EventStoreConnectionString, EventStoreTransportConnectionString>())
            {
                Console.WriteLine("Press any [Enter] to close the Retail Host.");
                Console.ReadLine();
            }
        }
    }
}
