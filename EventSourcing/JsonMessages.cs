using System;
using Newtonsoft.Json;

namespace EventSourcing
{
    public class JsonMessage
    {
        public JsonContent Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public Type NotificationType { get; set; }
        public Type SubscriptionType { get; set; }
    }

    public static class JsonEventStoreMessageHandler<TPersistence> where TPersistence : class
    {
        public static void Handle(JsonMessage message)
        {
            var subscriberMessage = new SubscriberMessage
            {
                Subscription = (Subscription)JsonConvert.DeserializeObject(message.Subscription.Value, message.SubscriptionType),
                Notification = (IDomainEvent)JsonConvert.DeserializeObject(message.NotificationContent.Value, message.NotificationType)
            };

            var handle = EventStore<TPersistence>.Submit(Post);

            handle(subscriberMessage);
        }

        public static Post Post = messages => {};
    }
}