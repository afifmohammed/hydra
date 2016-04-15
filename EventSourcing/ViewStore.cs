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
                        Channel<TEndpoint>.Push(
                            messageToConsumer,
                            consumersBySubscription,
                            notificationsByCorrelations,
                            () => DateTimeOffset.Now,
                            connection));
    }

    public static class AdoNetStorage<TViewStore, TEventStore> 
        where TViewStore : class
        where TEventStore : class
    {
        public static void Post(MessageToConsumer<AdoNetTransaction<TViewStore>> message)
        {
            using(var endpoint = new AdoNetTransaction<TEventStore>())
                ViewStore<AdoNetTransaction<TViewStore>>.PostAndCommit
                (
                    message,
                    ConsumersBySubscription,
                    EventStore<AdoNetTransaction<TEventStore>>.NotificationsByCorrelations(endpoint),
                    AdoNetTransaction<TViewStore>.CommitWork()
                );
        }

        public static ConsumersBySubscription<AdoNetTransaction<TViewStore>> ConsumersBySubscription { get; set; }
    }
}