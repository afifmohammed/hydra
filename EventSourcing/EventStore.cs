using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public delegate Func<IEnumerable<Correlation>, int> PublisherVersionByPublisherDataContractCorrelationsFunction<in TEndpointConnection>(
        TEndpointConnection connection) 
        where TEndpointConnection : EndpointConnection;

    public delegate IEnumerable<SerializedNotification> NotificationsByCorrelations(
        IEnumerable<Correlation> correlation);

    public delegate NotificationsByCorrelations NotificationsByCorrelationsFunction<in TEndpointConnection>(
        TEndpointConnection connection) 
        where TEndpointConnection : EndpointConnection;

    public delegate Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersionAction<in TEndpointConnection>(
        TEndpointConnection connection) 
        where TEndpointConnection : EndpointConnection;
    
    public static class EventStore<TEndpointConnection> where TEndpointConnection : EndpointConnection
    {
        public static NotificationsByCorrelationsFunction<TEndpointConnection> NotificationsByCorrelationsFunction { get; set; }
        public static PublisherVersionByPublisherDataContractCorrelationsFunction<TEndpointConnection> PublisherVersionByPublisherDataContractCorrelationsFunction { get; set; }
        public static SaveNotificationsByPublisherAndVersionAction<TEndpointConnection> SaveNotificationsByPublisherAndVersionAction { get; set; }
        public static CommitWork<TEndpointConnection> CommitEventStoreConnection { get; set; }
        public static Post Post = messages => { };

        public static Handle Handle = message => HandleAndCommitAndPost
        (
            message, 
            EventStore.PublishersBySubscription,
            HandlerWithNoSideEffects.Handle, 
            NotificationsByCorrelationsFunction,
            PublisherVersionByPublisherDataContractCorrelationsFunction,
            SaveNotificationsByPublisherAndVersionAction,
            CommitEventStoreConnection, 
            Post
        );

        static void HandleAndCommitAndPost(
            SubscriberMessage message,
            PublishersBySubscription publishersBySubscription,
            Handler handler,
            NotificationsByCorrelationsFunction<TEndpointConnection> notificationsByCorrelationsFunction,
            PublisherVersionByPublisherDataContractCorrelationsFunction<TEndpointConnection> publisherVersionByPublisherDataContractCorrelationsFunction,
            SaveNotificationsByPublisherAndVersionAction<TEndpointConnection> saveNotificationsByPublisherAndVersionAction,
            CommitWork<TEndpointConnection> commitWork, 
            Post post)
        {
            var list = new List<SubscriberMessage>();

            commitWork(connection =>
            {
                handler(
                    message,
                    publishersBySubscription,
                    notificationsByCorrelationsFunction(connection),
                    publisherVersionByPublisherDataContractCorrelationsFunction(connection),
                    () => DateTimeOffset.Now,
                    saveNotificationsByPublisherAndVersionAction(connection),
                    messages => list.AddRange(messages));
            });

            post(list);
        }
    }

    public static class EventStore
    {
        static EventStore()
        {
            PublishersBySubscription = new PublishersBySubscription();
        }

        public static PublishersBySubscription PublishersBySubscription { get; set; }

        public static void Register(params PublishersBySubscription[][] subscriptions)
        {
            PublishersBySubscription = PublishersBySubscription ?? new PublishersBySubscription();

            foreach (var kvp in subscriptions
                .SelectMany(x => x)
                .SelectMany(subscription => subscription))
            {
                PublishersBySubscription.Add(kvp.Key, kvp.Value);
            }
        }
    }


}