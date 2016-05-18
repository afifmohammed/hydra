using System.Linq;

namespace EventSourcing
{
    public static class SubscriberRegistration
    {
        public static EventStoreConfiguration ConfigureSubscriptions(this EventStoreConfiguration config, params PublisherSubscriptions[] subscriptions)            
        {
            EventStore.Register(subscriptions.Select(x => x.PublisherBySubscription).ToArray());
            return config;
        }
    }
}
