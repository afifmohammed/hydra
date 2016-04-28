using System;
using AdoNet;
using EventSourcing;
using Newtonsoft.Json;

namespace InventoryStockManager
{
    public class JsonMailboxMessage
    {
        public JsonContent Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public Type NotificationType { get; set; }
        public Type SubscriptionType { get; set; }
    }

    public class JsonMessageMailbox
    {
        public void Route(JsonMailboxMessage message)
        {
            var subscriberMessage = new SubscriberMessage
            {
                Subscription = (Subscription)JsonConvert.DeserializeObject(message.Subscription.Value, message.SubscriptionType),
                Notification = (IDomainEvent)JsonConvert.DeserializeObject(message.NotificationContent.Value, message.NotificationType)
            };

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Route(subscriberMessage);
        }
    }
}