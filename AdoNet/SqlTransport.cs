using System;
using EventSourcing;

namespace AdoNet
{
    public static class SqlTransport
    {
        public static void Initialize<EventStoreConnectionStringName, HangfireConnectionStringName>(Func<string, string> connectionString) 
            where EventStoreConnectionStringName : class
            where HangfireConnectionStringName : class
        {
            Hangfire.Initialize<HangfireConnectionStringName, EventStoreConnectionStringName>();

            PostBox<AdoNetTransactionScope>.CommitTransportConnection = AdoNetTransactionScope.Commit();

            PostBox<AdoNetTransactionScope>.Enqueue = Hangfire.Enqueue;
        }
    }
}