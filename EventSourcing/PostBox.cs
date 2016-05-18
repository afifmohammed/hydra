using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public delegate void Notify(IDomainEvent notification);

    public delegate void NotifyViaPost(
        IDomainEvent notification,
        SubscriberMessagesByNotification subscriberMessagesByNotification,
        Post post);

    public delegate IEnumerable<SubscriberMessage> SubscriberMessagesByNotification(IDomainEvent notification);

    public delegate void Post(IEnumerable<SubscriberMessage> messages);

    public delegate void Enqueue<in TEndpointConnection>(TEndpointConnection endpoint, IEnumerable<SubscriberMessage> messages) where TEndpointConnection : EndpointConnection;

    public static class PostBox<TEndpointConnection>
        where TEndpointConnection : EndpointConnection
    {
        public static Enqueue<TEndpointConnection> Enqueue { get; set; }
        public static CommitWork<TEndpointConnection> CommitTransportConnection { get; set; }

        public static Notify Drop = notification => 
            PostBox.NotifyViaPost
            (
                notification, 
                domainEvent => PostBox.SubscriberMessagesByNotificationList.SelectMany(subscriberMessagesByNotification => subscriberMessagesByNotification(domainEvent)), 
                Post
            );

        public static Post Post = messages => CommitTransportConnection(endpoint => Enqueue(endpoint, messages));
    }

    public static class PostBox
    {
        public static readonly List<SubscriberMessagesByNotification> SubscriberMessagesByNotificationList =
            new List<SubscriberMessagesByNotification>()
                .With(x => x.Add(e => PublisherChannel.PrepareMessages(e, EventStore.PublishersBySubscription)));

        public static NotifyViaPost NotifyViaPost = (notification, subscriberMessagesByNotification, post) =>
            post(subscriberMessagesByNotification(notification));
    }
}