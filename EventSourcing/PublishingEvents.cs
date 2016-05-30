using System.Linq;

namespace Hydra.Core
{
    public static class PostBox<TQueueProvider>
        where TQueueProvider : IProvider
    {
        public static Enqueue<TQueueProvider> Enqueue { get; set; }
        public static CommitWork<TQueueProvider> CommitWork { get; set; }

        public static Notify Drop = 
            getSubscriptions =>
                notifications => 
                    Post(notifications.SelectMany(notification => notification.SubscriberMessages(getSubscriptions())));
            
        public static Post Post = messages => CommitWork(provider => Enqueue(provider, messages));
    }
}