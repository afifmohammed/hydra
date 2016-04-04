using System;
using System.Collections.Generic;
using System.Transactions;
using EventSourcing;

namespace Client
{
    public static class Mailbox
    {
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
                { typeof(PublisherNotification).Contract(), m => EventStore.Post((PublisherNotification)m) }
            };

            routes[message.Contract()](message);
        }
    }
}
