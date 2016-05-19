using AdoNet;

namespace SerializedInvocation
{
    public class EventStoreConfiguration<TTransport, TEventStore> : EventStoreConfiguration<TEventStore> where TEventStore : class where TTransport : class
    {
        public static EventStoreConfiguration<TEventStore> CreateWithTransport()
        {
            return (EventStoreConfiguration<TEventStore>) Create().ConfigureTransport<TTransport, TEventStore>();
        }
    }
}