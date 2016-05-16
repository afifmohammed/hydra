using System;
using System.Linq;
using AdoNet;
using EventSourcing;
using Hangfire;
using RetailDomain.Inventory;
using RetailDomain.Refunds;

namespace PublisherHost
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlEventStore.Initialize<ApplicationStore>(ConnectionString.ByName);
            SqlTransport.Initialize<ApplicationStore, EventStoreTransport>(ConnectionString.ByName);

            foreach (var element in new PublishersBySubscription()
                .Union(InventoryItemStockHandler.Subsriptions().PublisherBySubscription)
                .Union(RefundProductOrderHandler.Subscriptions().PublisherBySubscription))
            {
                EventStore.PublishersBySubscription.Add(element.Key, element.Value);
            }

            using (new BackgroundJobServer())
            { 
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }
}
