using System;
using System.Collections.Generic;
using System.Configuration;
using AdoNet;
using EventSourcing;
using Hangfire;
using Hangfire.SqlServer;
using InventoryStockManager.Domain;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

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

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.CommitEventStoreConnection =
                AdoNetTransaction<ApplicationStore>.CommitWork(ConnectionString.ByName);

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.CommitTransportConnection =
                AdoNetTransactionScope.Commit();

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: "EventStoreTransport",
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            EventStore<AdoNetTransaction<ApplicationStore>>.NotificationsByCorrelations =
                t => SqlQueries.NotificationsByCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.PublisherVersionByPublisherDataContractCorrelations =
                t => SqlQueries.PublisherVersionByContractAndCorrelations(t.Value);

            EventStore<AdoNetTransaction<ApplicationStore>>.SaveNotificationsByPublisherAndVersion =
                t => SqlQueries.SaveNotificationsByPublisherAndVersion(t.Value);

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Enqueue = (endpoint, messages) =>
            {
                foreach (var subscriberMessage in messages)
                {
                    MappersByContract.Mappers[new TypeContract(subscriberMessage.Notification)] = content => 
                        (IDomainEvent)JsonConvert.DeserializeObject(content.Value, subscriberMessage.Notification.GetType());

                    var message = new AdoNetMailboxMessage
                    {
                        NotificationContent = new JsonContent(subscriberMessage.Notification),
                        NotificationContract = subscriberMessage.Notification.Contract(),
                        Subscription = subscriberMessage.Subscription
                    };

                    BackgroundJob.Enqueue(() => new AdoNetMailbox().Route(message));
                }
            };

            using (var host = new NancyHost(uri))
            {
                var svr = new BackgroundJobServer();
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();

                svr.Dispose();
            }
        }
    }

    public static class MappersByContract
    {
        public static IDictionary<TypeContract, Func<JsonContent, IDomainEvent>> Mappers = new Dictionary<TypeContract, Func<JsonContent, IDomainEvent>>();
    }

    public class AdoNetMailboxMessage
    {
        public Subscription Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public TypeContract NotificationContract { get; set; }
    }

    public class AdoNetMailbox
    {
        public void Route(AdoNetMailboxMessage message)
        {
            var subscriberMessage = new SubscriberMessage {Subscription = message.Subscription, Notification = MappersByContract.Mappers[message.NotificationContract](message.NotificationContent)};
            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Route(subscriberMessage);
        }
    }

    static class ConnectionString
    {
        public static Func<string, string> ByName = connectionStringName => ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
    }   
}
