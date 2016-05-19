using EventSourcing;

namespace AdoNet
{
    public class EventStoreConfiguration<TEventStore> : EventStoreConfiguration where TEventStore : class
    {
        public static EventStoreConfiguration<TEventStore> Create()
        {
            return new EventStoreConfiguration<TEventStore>();
        }

        public EventStoreConfiguration<TEventStore> ConfigurePublishers()
        {
            return (EventStoreConfiguration<TEventStore>)this.ConfigurePublishers<TEventStore>();
        }

        public EventStoreConfiguration<TEventStore> ConfigurePublishingNotifications()
        {
            return (EventStoreConfiguration<TEventStore>)this.ConfigurePublishingNotifications<TEventStore>();
        }
    }
}