using System;
using AdoNet;
using EventSourcing;
using Newtonsoft.Json;

namespace InventoryStockManager
{
    public class JsonMailboxMessage
    {
        public Subscription Subscription { get; set; }
        public JsonContent NotificationContent { get; set; }
        public string NotificationType { get; set; }
    }

    public class JsonMessageMailbox
    {
        public void Route(JsonMailboxMessage message)
        {
            var subscriberMessage = new SubscriberMessage
            {
                Subscription = message.Subscription,
                Notification = (IDomainEvent)JsonConvert.DeserializeObject(message.NotificationContent.Value, Type.GetType(message.NotificationType))
            };

            Mailbox<AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Route(subscriberMessage);
        }
    }
}