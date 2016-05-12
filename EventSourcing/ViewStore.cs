using System;

namespace EventSourcing
{
    public delegate void PostAndCommit<TEndpoint>(
        MessageToConsumer<TEndpoint> messageToConsumer,
        ConsumersBySubscription<TEndpoint> consumersBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        CommitWork<TEndpoint> commit) 
        where TEndpoint : class;

    public static class ViewStore<TEndpoint> where  TEndpoint : class
    {
        public static PostAndCommit<TEndpoint> PostAndCommit = (
            messageToConsumer, 
            consumersBySubscription, 
            notificationsByCorrelations, 
            commit) => 
                commit(
                    connection => 
                        ConsumerChannel<TEndpoint>.Handler(
                            messageToConsumer,
                            consumersBySubscription,
                            notificationsByCorrelations,
                            () => DateTimeOffset.Now,
                            connection));
    }
}