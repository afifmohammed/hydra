using System;

namespace EventSourcing
{
    public class JsonMessage
    {
        public JsonMessage()
        {
            HandlerAddress = "default";
        }

        public JsonMessage(SubscriberMessage subscriberMessage)
        {
            NotificationContent = new JsonContent(subscriberMessage.Notification);
            NotificationType = subscriberMessage.Notification.GetType();
            Subscription = new JsonContent(subscriberMessage.Subscription);
            SubscriptionType = subscriberMessage.Subscription.GetType();
            HandlerAddress = subscriberMessage.Subscription.SubscriberDataContract.Value;
        }

        public string HandlerAddress { get; set; }
        public JsonContent Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public Type NotificationType { get; set; }
        public Type SubscriptionType { get; set; }
    }

    public static class JsonEventStoreMessageHandler<TPersistence> where TPersistence : class
    {
        public static void Handle(JsonMessage message)
        {
            var handle = EventStore<TPersistence>.Handler(Post);

            handle(new SubscriberMessage(message));
        }

        public static Post Post = messages => {};
    }
}