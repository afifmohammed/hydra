using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EventSourcing;

namespace Client
{
    public delegate void Commit(MessageToPublisher messageToPublisher);
    public delegate Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> NotificationsByCorrelations<in TConnection>(TConnection connection);
    
    public static class EventStore<TConnection>
    {
        public static NotificationsByCorrelations<TConnection> NotificationsByCorrelations { get; set; }
        public static ConsumersBySubscription<TConnection> ConsumersBySubscription { get; set; }
    }

    public static class EventStore
    {
        public static Commit Commit = messageToPublisher =>
        {
            using (var c = new SqlConnection("EventStore").With(x => x.Open()))
            using (var t = c.BeginTransaction())
            {
                Channel.Push(
                    messageToPublisher,
                    EventStore.PublishersBySubscription(),
                    EventStore.NotificationsByCorrelations(t),
                    EventStore.PublisherVersionByPublisherDataContractCorrelations(t),
                    () => DateTimeOffset.Now,
                    EventStore.SaveNotificationsByPublisherAndVersion(t),
                    publisherNotifications => Mailbox.Post(publisherNotifications.Cast<SubscriberMessage>()));

                t.Commit();
            }
        };

        public static PublishersBySubscription PublishersBySubscription()
        {
            // todo: select many from calling every handler from the domain
            return new PublishersBySubscription();
        }

        public static Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> NotificationsByCorrelations(IDbTransaction transaction)
        {
            // todo: fetch the notifications from the database by using the correlations as the criteria
            return correlations => new List<SerializedNotification>();
        }

        public static Func<IEnumerable<Correlation>, int> PublisherVersionByPublisherDataContractCorrelations(IDbTransaction transaction)
        {
            // todo: fetch the publisher version from the database by using the correlations as the criteria
            return correlations => 1;
        }

        public static Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersion(IDbTransaction transaction)
        {
            // todo: save notifications to database and update publisher version
            return notifications => { };
        }
    }
}