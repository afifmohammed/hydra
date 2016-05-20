using AdoNet;
using System;
using System.Linq;
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
                .ConfigureEventStoreWithTransportConnection<EventStoreConnectionString, EventStoreTransportConnectionString>()
                .ConfigureTransport()
                .ConfigurePublishers()
                .ConfigurePublishingNotifications()
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
