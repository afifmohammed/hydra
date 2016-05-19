using System;
using System.Linq;
using AdoNet;
using EventSourcing;
using Hangfire;
using RetailDomain.Inventory;
using RetailDomain.Refunds;
using SerializedInvocation;

namespace PublisherHost
{
    class Program
    {
        static void Main(string[] args)
        {
            new EventStoreConfiguration()
                .ConfigurePublishers<EventStoreConnectionString>()
                .ConfigurePublishingNotifications<EventStoreConnectionString>()
                .ConfigureTransport<EventStoreTransportConnectionString>()
                .ConfigureSubscriptions(
                    InventoryItemStockHandler.Subscriptions(),
                    RefundProductOrderHandler.Subscriptions());

            var options = new BackgroundJobServerOptions
            {
                Queues = EventStore.PublishersBySubscription.Keys.Select(x => x.SubscriberDataContract.Value.ToLower()).ToArray()
            };

            using (new BackgroundJobServer(options))
            {
                Console.WriteLine("Press any [Enter] to close the Publisher Host.");
                Console.ReadLine();
            }
        }
    }
}
