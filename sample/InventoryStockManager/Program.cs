using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
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

            EventStore<AdoNetTransaction<ApplicationStore>>.NotificationsByCorrelations = connection => 
                correlations => connection.Value.Connection.Query<SerializedNotification>(
                    sql:new NotificationsByCorrelationQuery(correlations).ToString(), 
                    transaction:connection.Value, 
                    param: new NotificationsByCorrelationQuery(correlations).Parameters);

            EventStore<AdoNetTransaction<ApplicationStore>>.PublisherVersionByPublisherDataContractCorrelations = connection =>
                correlations => connection.Value.Connection.Query<int>(sql:"", transaction:connection.Value, param:new {}).FirstOrDefault();

            EventStore<AdoNetTransaction<ApplicationStore>>.SaveNotificationsByPublisherAndVersion = connection =>
                notificationsByPublisherAndVersion =>
                {
                    if(connection.Value.Connection.Query<int>(sql: "", transaction: connection.Value, param: new { }).FirstOrDefault() != 1)
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

    class NotificationsByCorrelationQuery
    {
        private readonly StringBuilder _query;
        public NotificationsByCorrelationQuery(IEnumerable<Correlation> correlations)
        {
            var kvps = new Dictionary<string, object>();
            var counter = 1;
            foreach (var correlation in correlations)
            {
                kvps.Add("correlationContract" + counter, correlation.Contract);
                kvps.Add("correlationPropertyName" + counter, correlation.PropertyName);
                kvps.Add("correlationPropertyValue" + counter, correlation.PropertyValue.Value);
                counter++;
            }

            var eo = new ExpandoObject();

            foreach (var kvp in kvps)
            {
                ((ICollection<KeyValuePair<string, object>>)eo).Add(kvp);
            }

            Parameters = (dynamic) eo;


        }

        public override string ToString()
        {
            return _query.ToString();
        }

        public readonly object Parameters;
    }
}
