using System;
using Hydra.Core;
using Newtonsoft.Json;

namespace Hydra.SerializedInvocation
{
    public static class JsonMessageHandler
    {
        [UseQueueFromParameter(0)]
        public static void Handle(JsonMessage message)
        {
            HandleInstance(message.AsSubscriberMessage());
        }

        /// <summary>
        /// To be overriden at the server where the <see cref="Handle"/> job is deserialized and invoked 
        /// </summary>
        public static Action<SubscriberMessage> HandleInstance = m => { };
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
            HandlerAddress = subscriberMessage.Subscription.SubscriberDataContract.Value.ToLower();
        }

        public string HandlerAddress { get; set; }
        public JsonContent Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public Type NotificationType { get; set; }
        public Type SubscriptionType { get; set; }
    }

    public static class JsonMessageConverter
    {
        public static SubscriberMessage AsSubscriberMessage(this JsonMessage message)
        {
            var subscriberMessage = new SubscriberMessage
            {
                Subscription = (Subscription) JsonConvert.DeserializeObject(
                    message.Subscription.Value,
                    message.SubscriptionType),
                Notification = (INotification) JsonConvert.DeserializeObject(
                    message.NotificationContent.Value,
                    message.NotificationType)
            };

            return subscriberMessage;
        }
    }    
}