using AdoNet;
using System;
using EventSourcing;
using Hangfire;
using SerializedInvocation;

namespace PublisherHost
{
    class Program
    {
        static void Main(string[] args)
        {
            new EventStoreConfiguration()
                .ConfigureEventStoreConnection<EventStoreConnectionString>()
                .ConfigurePublishers()
                .ConfigurePushNotifications()
                .ConfigureTransport<EventStoreTransportConnectionString>();

            var options = new BackgroundJobServerOptions().With(x => x.QueuePerSubscriber());

            using (new BackgroundJobServer(options))
            {
                Console.WriteLine("Press any [Enter] to close the Publisher Host.");
                Console.ReadLine();
            }
        }
    }
}
