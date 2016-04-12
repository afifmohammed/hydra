using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using EventSourcing;

namespace Client
{
    public static class Mailbox
    {
        public static void Post(IDomainEvent notification)
        {
            Post
            (
                new List<Message>()
                    .With(x => x.AddRange(Channel.PrepareMessages(notification, EventStore.PublishersBySubscription()).Cast<Message>()))
                    .With(x => x.AddRange(Channel.PrepareMessages(notification, EventStore.SubscribersBySubscription<ViewStoreConnection>()).Cast<Message>()))
            );
        }

        public static void Post(IEnumerable<Message> messages)
        {
            using (var transaction = new TransactionScope())
            {
                foreach (var message in messages)
                {
                    Hangfire.BackgroundJob.Enqueue(() => Mailbox.Route(message));
                }

                transaction.Complete();
            }
        }

        static void Route(Message message)
        {
            var routes = new Dictionary<TypeContract, Action<Message>>
            {
                { typeof(MessageToPublisher).Contract(), m => EventStore.Post((MessageToPublisher)m) },
                { typeof(MessageToConsumer<ViewStoreConnection>).Contract(), m => ViewStore.Post((MessageToConsumer<ViewStoreConnection>)m) }
            };

            routes[message.Contract()](message);
        }
    }
}