using System;
using EventSourcing;

namespace SerializedInvocation
{
    public static class JsonMessageHandler
    {
        [UseQueueFromParameter(0)]
        public static void Handle(JsonMessage message)
        {
            HandleInstance(message);
        }

        public static Action<JsonMessage> HandleInstance = m => { };
    }

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
}