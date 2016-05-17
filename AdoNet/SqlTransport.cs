using System;
using EventSourcing;

namespace AdoNet
{
    public static class SqlTransport
    {
        public static void Initialize<TEventStoreName, TTransportStoreName>(Func<string, string> connectionString) where TEventStoreName : class
        {
            PostBox<AdoNetTransactionScope>.CommitTransportConnection = AdoNetTransactionScope.Commit();

            PostBox<AdoNetTransactionScope>.Enqueue = Hangfire<TTransportStoreName>.Enqueue<TEventStoreName>;
        }
    }
}