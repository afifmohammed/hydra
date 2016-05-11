﻿using System;
using AdoNet;
using Hangfire;
using Hangfire.SqlServer;
using Nancy;
using Nancy.Hosting.Self;

namespace WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlEventStore.Initialize<ApplicationStore>(ConnectionString.ByName, message => BackgroundJob.Enqueue(message));

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: "EventStoreTransport",
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });

            var uri = new Uri("http://localhost:3785");

            using (var host = new NancyHost(
                uri, 
                new DefaultNancyBootstrapper(), 
                new HostConfiguration { UrlReservations = new UrlReservations() { CreateAutomatically = true } }))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }
}