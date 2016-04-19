using System;
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

    public delegate void Route(SubscriberMessage message);
    public delegate void Enqueue<in TEndpoint>(TEndpoint endpoint, IEnumerable<SubscriberMessage> messages) where TEndpoint : class;

    public static class Mailbox<TEventStoreEndpoint, TTransportEndpoint>
        where TEventStoreEndpoint : class
        where TTransportEndpoint : class
    {
        public static Enqueue<TTransportEndpoint> Enqueue { get; set; }
        public static CommitWork<TEventStoreEndpoint> CommitEventStoreConnection { get; set; }
        public static CommitWork<TTransportEndpoint> CommitTransportConnection { get; set; }

        public static readonly List<SubscriberMessagesByNotification> SubscriberMessagesByNotification = 
            new List<SubscriberMessagesByNotification>()
                .With(x => x.Add(e => PublisherChannel.PrepareMessages(e, EventStore.PublishersBySubscription).Cast<SubscriberMessage>()));

        public static Notify Notify = notification => 
            NotifyViaPost
            (
                notification, 
                n => SubscriberMessagesByNotification.SelectMany(x => x(n)), 
                Post
            );

        public static NotifyViaPost NotifyViaPost = (notification, subscriberMessagesByNotification, post) =>
            post(subscriberMessagesByNotification(notification));
        
        public static Post Post = messages => CommitTransportConnection(endpoint => Enqueue(endpoint, messages));

        public static readonly IDictionary<TypeContract, Action<SubscriberMessage>> SubscriberRoutes = 
            new Dictionary<TypeContract, Action<SubscriberMessage>>
            {
                {
                    typeof (MessageToPublisher).Contract(),
                    m => EventStore<TEventStoreEndpoint, TTransportEndpoint>.NotifyPublisher((MessageToPublisher) m, CommitEventStoreConnection)
                }
            };

        public static Route Route = message => SubscriberRoutes[message.Contract()](message);
    }
}