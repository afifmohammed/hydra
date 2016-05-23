namespace EventSourcing
{
    /// <summary>
    /// Placed holder class off which configurations for the event store can hang as extension methods
    /// </summary>    
    public class EventStoreConfiguration
    {}

    public class EventStoreConfiguration<TEventStore> : EventStoreConfiguration where TEventStore : class
    {    }

    public class EventStoreConfiguration<TEventStore, TTransport> : EventStoreConfiguration<TEventStore> where TEventStore : class where TTransport : class
    {}
}
