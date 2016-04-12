﻿using System;
using System.Data;
using System.Data.SqlClient;
using EventSourcing;

namespace Client
{
    public class ViewStoreConnection : Unit<IDbTransaction>
    {
        public ViewStoreConnection(IDbTransaction transaction)
        {
            Value = transaction;
        }
        public IDbTransaction Value { get; }
    }

    public static class ViewStore
    {
        public static void Post(MessageToConsumer<ViewStoreConnection> messageToConsumer)
        {
            using (var c = new SqlConnection("ViewStore").With(x => x.Open()))
            using (var t = c.BeginTransaction())
            {
                Channel.Push(
                    messageToConsumer,
                    EventStore.SubscribersBySubscription<ViewStoreConnection>(),
                    EventStore.NotificationsByCorrelations(t),
                    () => DateTimeOffset.Now,
                    new ViewStoreConnection(t));

                t.Commit();
            }
        }
    }
}