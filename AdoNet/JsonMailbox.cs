using System;
using EventSourcing;
using Newtonsoft.Json;

namespace AdoNet
{
    public class JsonMailboxMessage
    {
        public JsonContent Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public Type NotificationType { get; set; }
        public Type SubscriptionType { get; set; }
    }

    public static class JsonMessageMailbox<TStore> where TStore : class
    {
        public static void Submit(JsonMailboxMessage message)
        {
            var subscriberMessage = new SubscriberMessage
            {
                Subscription = (Subscription)JsonConvert.DeserializeObject(message.Subscription.Value, message.SubscriptionType),
                Notification = (IDomainEvent)JsonConvert.DeserializeObject(message.NotificationContent.Value, message.NotificationType)
            };

            var handle = EventStore<AdoNetTransaction<TStore>>.Submit(Post);

            handle(subscriberMessage);
        }

        public static Post Post = messages => {};
    }
}