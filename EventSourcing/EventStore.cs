using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public delegate void Handle(SubscriberMessage message);
    public delegate void NotifyPublisher<in TEndpointConnection>(MessageToPublisher messageToPublisher, Handler handler, CommitWork<TEndpointConnection> commitWork) where TEndpointConnection : EndpointConnection;
    public delegate Func<IEnumerable<Correlation>, int> PublisherVersionByPublisherDataContractCorrelations<in TEndpointConnection>(TEndpointConnection connection) where TEndpointConnection : EndpointConnection;
    public delegate IEnumerable<SerializedNotification> NotificationsByCorrelations(IEnumerable<Correlation> correlation);
    public delegate NotificationsByCorrelations NotificationsByCorrelations<in TEndpointConnection>(TEndpointConnection connection) where TEndpointConnection : EndpointConnection;
    public delegate Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersion<in TEndpointConnection>(TEndpointConnection connection) where TEndpointConnection : EndpointConnection;
    
    public static class EventStore<TEndpointConnection> where TEndpointConnection : EndpointConnection
    {
        public static NotificationsByCorrelations<TEndpointConnection> NotificationsByCorrelations { get; set; }
        public static PublisherVersionByPublisherDataContractCorrelations<TEndpointConnection> PublisherVersionByPublisherDataContractCorrelations { get; set; }
        public static SaveNotificationsByPublisherAndVersion<TEndpointConnection> SaveNotificationsByPublisherAndVersion { get; set; }

        public static Func<Post, NotifyPublisher<TEndpointConnection>> NotifyPublisher = post => 
            (messageToPublisher, handler, commitWork) => NotifyPublisherAndPost(messageToPublisher, handler, commitWork, post);

        public static CommitWork<TEndpointConnection> CommitEventStoreConnection { get; set; }

        public static Handle Handle = message => HandleAndPost(message, Post);

        public static Post Post = messages => { };

        private static void NotifyPublisherAndPost(MessageToPublisher messageToPublisher, Handler handler, CommitWork<TEndpointConnection> commitWork, Post post)
        {
            var list = new List<MessageToPublisher>();

            commitWork(connection =>
            {
                handler(
                    messageToPublisher,
                    EventStore.PublishersBySubscription,
                    NotificationsByCorrelations(connection),
                    PublisherVersionByPublisherDataContractCorrelations(connection),
                    () => DateTimeOffset.Now,
                    SaveNotificationsByPublisherAndVersion(connection),
                    messages => list.AddRange(messages));
            });

            post(list);
        }

        private static void HandleAndPost(SubscriberMessage message, Post post)
        {
            var messageToPublisher = new MessageToPublisher
            {
                Notification = message.Notification,
                Subscription = message.Subscription
            };

            var notifyPublisher = NotifyPublisher(post);

            notifyPublisher(messageToPublisher, PublisherChannel.Handler, CommitEventStoreConnection);
        }
    }

    public static class EventStore
    {
        static EventStore()
        {
            PublishersBySubscription = new PublishersBySubscription();
        }

        public static PublishersBySubscription PublishersBySubscription { get; set; }

        public static void Register(params IDictionary<Subscription, Publisher>[] subscriptions)
        {
            PublishersBySubscription = PublishersBySubscription ?? new PublishersBySubscription();
            foreach (var subscription in subscriptions)
                foreach (var kvp in subscription)
                    PublishersBySubscription.Add(kvp.Key, kvp.Value);
        }        
    }


}