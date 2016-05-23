using System;
using System.Collections.Generic;

namespace EventSourcing
{
    public delegate Action<IDomainEvent> Notify(IEnumerable<Subscription> subscriptions);

    public delegate void Post(IEnumerable<SubscriberMessage> messages);

    public delegate void Enqueue<in TProvider>(
        TProvider provider, 
        IEnumerable<SubscriberMessage> messages) 
        where TProvider : IProvider;

    public static class PostBox<TProvider>
        where TProvider : IProvider
    {
        public static Enqueue<TProvider> Enqueue { get; set; }
        public static CommitWork<TProvider> CommitWork { get; set; }

        public static Notify Drop = 
            subscriptions =>
                notification => 
                    Post(SubscriberMessages.By(notification, subscriptions));
            
        public static Post Post = messages => CommitWork(endpoint => Enqueue(endpoint, messages));
    }
}