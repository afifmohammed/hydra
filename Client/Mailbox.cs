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
                    Hangfire.BackgroundJob.Enqueue(() => Mailbox.Post(message));
                }

                transaction.Complete();
            }
        }

        public static void Post(Message message)
        {
            var routes = new Dictionary<TypeContract, Action<Message>>
            {
                { typeof(PublisherNotification).Contract(), m => EventStore.Post((PublisherNotification)m) },
                { typeof(SubscriberNotification<ViewStoreConnection>).Contract(), m => ViewStore.Post((SubscriberNotification<ViewStoreConnection>)m) }
            };

            routes[message.Contract()](message);
        }
    }
}