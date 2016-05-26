using System.Linq;

namespace Hydra.Core
{
    public static class PostBox<TProvider>
        where TProvider : IProvider
    {
        public static Enqueue<TProvider> Enqueue { get; set; }
        public static CommitWork<TProvider> CommitWork { get; set; }

        public static Notify Drop = 
            getSubscriptions =>
                notifications => 
                    Post(notifications.SelectMany(notification => notification.SubscriberMessages(getSubscriptions())));
            
        public static Post Post = messages => CommitWork(provider => Enqueue(provider, messages));
    }
}