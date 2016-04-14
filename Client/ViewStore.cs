using System;
using System.Data;
using System.Data.SqlClient;
using EventSourcing;

namespace Client
{
    public delegate void PostAndCommit<TConnection>(
        MessageToConsumer<TConnection> messageToConsumer,
        ConsumersBySubscription<TConnection> consumersBySubscription,
        NotificationsByCorrelations<TConnection> notificationsByCorrelations,
        Action<Action<TConnection>> commit);

    public static class ViewStore<TConnection>
    {
        public static PostAndCommit<TConnection> PostAndCommit = 
        (
            messageToConsumer,
            consumersBySubscription,
            notificationsByCorrelations,
            commit
        ) => 
            commit(
                connection => 
                    Channel<TConnection>.Push(
                        messageToConsumer,
                        consumersBySubscription,
                        notificationsByCorrelations(connection),
                        () => DateTimeOffset.Now,
                        connection));
    }

    public static class AdoNetViewStore
    {
        public static void Post(MessageToConsumer<AdoNetViewStoreConnection> message)
        {
            ViewStore<AdoNetViewStoreConnection>.PostAndCommit
            (
                message,
                EventStore<AdoNetViewStoreConnection>.ConsumersBySubscription,
                EventStore<AdoNetViewStoreConnection>.NotificationsByCorrelations,
                doWork =>
                {
                    using (var c = new SqlConnection("ViewStore").With(x => x.Open()))
                    using (var t = c.BeginTransaction())
                    {
                        doWork(new AdoNetViewStoreConnection(t));
                        t.Commit();
                    }
                }
            );
        }
    }

    public class AdoNetViewStoreConnection : Unit<IDbTransaction>
    {
        public AdoNetViewStoreConnection(IDbTransaction transaction)
        {
            Value = transaction;
        }
        public IDbTransaction Value { get; }
    }
}