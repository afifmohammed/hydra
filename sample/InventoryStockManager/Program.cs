using System;
using System.Linq;
using System.Transactions;
using EventSourcing;
using Hangfire;
using InventoryStockManager.Domain;
using Nancy.Hosting.Self;

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

            Mailbox<AdoNetTransaction<ApplicationStore>, TransactionScope>.CommitEventStoreConnection = AdoNetTransaction<ApplicationStore>.CommitWork();

            Mailbox<AdoNetTransaction<ApplicationStore>, TransactionScope>.CommitTransportConnection = work =>
            {
                using (var scope = new TransactionScope())
                {
                    work(scope);
                    scope.Complete();
                }
            };

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

    class ApplicationStore { }
    

}
