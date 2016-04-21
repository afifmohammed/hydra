using EventSourcing;

namespace AdoNet
{
    public static class AdoNetStorage<TViewStore, TEventStore>
        where TViewStore : class
        where TEventStore : class
    {
        public static void Post(MessageToConsumer<AdoNetTransaction<TViewStore>> message)
        {
            using (var endpoint = new AdoNetTransaction<TEventStore>())
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