using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace Tests
{
    static class Extensions
    {
        public static ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint> Notify<TSubscriberDataContract, TEndpoint>(
            this ConsumerContractSubscriptions<TSubscriberDataContract, TEndpoint> consumerContractSubscriptions,
            IDomainEvent notification,
            TEndpoint endpoint) 
            where TSubscriberDataContract : new()
        {
            consumerContractSubscriptions
                .ConsumerBySubscription[new Subscription(notificationContract: new TypeContract(notification), subscriberDataContract: new TypeContract(typeof (TSubscriberDataContract)))]
                (
                    notification, 
                    NotificationsByCorrelations(), 
                    () => DateTimeOffset.Now, 
                    endpoint
                );
            return consumerContractSubscriptions;
        }

        public static Func<IDomainEvent, IEnumerable<Func<IDomainEvent, NotificationsByPublisher>>> Given(
            this PublishersBySubscription subscriptions,
            params IDomainEvent[] given)
        {
            return n => subscriptions
                .Where(p => p.Key.NotificationContract.Equals(n.Contract())).Select(p => p.Value)
                .Select<Publisher, Func<IDomainEvent, NotificationsByPublisher>>
                (
                    function =>
                        notification =>
                            function(
                                notification,
                                NotificationsByCorrelations(given),
                                () => DateTimeOffset.Now)
                );

        }

        public static IEnumerable<NotificationsByPublisher> Notify<TNotification>(this
            Func<IDomainEvent, IEnumerable<Func<IDomainEvent, NotificationsByPublisher>>> publishers,
            TNotification notification)
            where TNotification : IDomainEvent
        {
            return publishers(notification)
                .Select(x => x(notification));
        }

        static Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>> NotificationsByCorrelations(params IDomainEvent[] notifications)
        {
            return correlations => notifications
                .Select(n => new
                {
                    Notification = new SerializedNotification
                    {
                        Contract = n.Contract(),
                        JsonContent = new JsonContent(n)
                    },
                    Correlations = n.Correlations()
                })
                .Where(n => correlations.Where(c => c.Contract.Equals(n.Contract())).All(c => n.Correlations.Any(nc => nc.Equals(c))))
                .Select(x => x.Notification);
        }
    }
}