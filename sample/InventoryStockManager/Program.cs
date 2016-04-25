using System;
using System.Data;
using System.Linq;
using System.Transactions;
using AdoNet;
using Dapper;
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

            EventStore<AdoNetTransaction<ApplicationStore>>.NotificationsByCorrelations = SqlQueries.NotificationsByCorrelations;

            EventStore<AdoNetTransaction<ApplicationStore>>.PublisherVersionByPublisherDataContractCorrelations = connection =>
                correlations => connection.Value.Connection.Query<int>(
                    sql:"", // todo:
                    transaction:connection.Value, 
                    param:new {} // todo:
                    ).FirstOrDefault();

            EventStore<AdoNetTransaction<ApplicationStore>>.SaveNotificationsByPublisherAndVersion = connection =>
                notificationsByPublisherAndVersion =>
                {
                    if(connection.Value.Connection.Query<int>(
                            sql: "", // todo:
                            transaction: connection.Value, 
                            param: new { } // todo:
                        ).FirstOrDefault() != 1)
                        throw new DBConcurrencyException();
                };

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
