using System.Collections.Generic;

namespace EventSourcing
{
    public class PublishersBySubscription : Dictionary<Subscription, Publisher>
    { }

    public class ConsumersBySubscription<TEndpoint> : Dictionary<Subscription, Consumer<TEndpoint>>
    { }

    public class ConsumersBySubscription<TEndpoint1, TEndpoint2> : Dictionary<Subscription, Consumer<TEndpoint1, TEndpoint2>>
    { }

    public struct Subscription
    {
        public Subscription(TypeContract notificationContract, TypeContract subscriberDataContract)
        {
            NotificationContract = notificationContract;
            SubscriberDataContract = subscriberDataContract;
        }

        public TypeContract NotificationContract { get; set; }
        public TypeContract SubscriberDataContract { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if ((obj is Subscription) == false) return false;

            return Equals((Subscription)obj);
        }

        public bool Equals(Subscription other)
        {
            return NotificationContract.Equals(other.NotificationContract) && SubscriberDataContract.Equals(other.SubscriberDataContract);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NotificationContract.GetHashCode() * 397) ^ SubscriberDataContract.GetHashCode();
            }
        }
    }
}