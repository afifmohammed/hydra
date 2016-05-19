using System;

namespace EventSourcing
{
    public static class ConsumerStore<TEndpointConnection, TEventStoreEndpointConnection> 
        where TEndpointConnection : EndpointConnection
        where TEventStoreEndpointConnection : EndpointConnection
    {
        public static ConsumersBySubscription<TEndpointConnection> ConsumersBySubscription { get; set; }
        public static CommitWork<TEndpointConnection> CommitEndpointConnection { get; set; }
        public static CommitWork<TEventStoreEndpointConnection> CommitEventStoreEndpointConnection { get; set; }
        public static NotificationsByCorrelationsFunction<TEventStoreEndpointConnection> NotificationsByCorrelationsFunction { get; set; }

        public static Handle Handle = message => HandleAndCommit
        (
            message,
            ConsumersBySubscription,
            HandlerWithSideEffectsTo<TEndpointConnection>.Handle,
            NotificationsByCorrelationsFunction,
            CommitEndpointConnection,
            CommitEventStoreEndpointConnection,
            () => DateTimeOffset.Now
        );

        static void HandleAndCommit(
            SubscriberMessage message,
            ConsumersBySubscription<TEndpointConnection> consumersBySubscription,
            Handler<TEndpointConnection> handler,
            NotificationsByCorrelationsFunction<TEventStoreEndpointConnection> notificationsByCorrelationsFunction,
            CommitWork<TEndpointConnection> commitEndpointConnection,
            CommitWork<TEventStoreEndpointConnection> commitEventStoreEndpointConnection,
            Func<DateTimeOffset> clock)
        {
            commitEndpointConnection
            (
                consumerEndpointConnection => commitEventStoreEndpointConnection
                (
                    eventStoreConnection => handler
                    (
                        message,
                        consumersBySubscription,
                        notificationsByCorrelationsFunction(eventStoreConnection),
                        clock,
                        consumerEndpointConnection
                    )
                )
            );
        }
    }
}