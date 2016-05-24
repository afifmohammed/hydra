using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public delegate Action<IEnumerable<IDomainEvent>> Notify(Func<IEnumerable<Subscription>> subscriptions);

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
            getSubscriptions =>
                notifications => 
                    Post(notifications.SelectMany(notification => SubscriberMessages.By(notification, getSubscriptions())));
            
        public static Post Post = messages => CommitWork(provider => Enqueue(provider, messages));
    }
}