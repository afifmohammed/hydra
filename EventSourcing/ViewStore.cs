using System;

namespace EventSourcing
{
    public delegate void PostAndCommit<TEndpointConnection>(
        MessageToConsumer<TEndpointConnection> messageToConsumer,
        ConsumersBySubscription<TEndpointConnection> consumersBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        CommitWork<TEndpointConnection> commit) 
        where TEndpointConnection : EndpointConnection;

    public static class ViewStore<TEndpointConnection> where  TEndpointConnection : EndpointConnection
    {
        public static PostAndCommit<TEndpointConnection> PostAndCommit = (
            messageToConsumer, 
            consumersBySubscription, 
            notificationsByCorrelations, 
            commit) => 
                commit(
                    connection => 
                        ConsumerChannel<TEndpointConnection>.Handler(
                            messageToConsumer,
                            consumersBySubscription,
                            notificationsByCorrelations,
                            () => DateTimeOffset.Now,
                            connection));
    }
}