using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;
using Hydra.Core.FluentInterfaces.Subscribers;

namespace Tests
{
    static class Extensions
    {
        public static ConsumerContractSubscriptions<TSubscriberDataContract, TProvider> Notify<TSubscriberDataContract, TProvider>(
            this ConsumerContractSubscriptions<TSubscriberDataContract, TProvider> consumerContractSubscriptions,
            IDomainEvent notification,
            TProvider provider) 
            where TSubscriberDataContract : new()
        {
            consumerContractSubscriptions
                .ExportersBySubscription[new Subscription(notificationContract: new TypeContract(notification), subscriberDataContract: new TypeContract(typeof (TSubscriberDataContract)))]
                (
                    notification, 
                    NotificationsByCorrelations(), 
                    () => DateTimeOffset.Now, 
                    provider
                );
            return consumerContractSubscriptions;
        }

        public static Func<INotification, IEnumerable<Func<INotification, NotificationsByPublisher>>> Given(
            this PublishersBySubscription subscriptions,
            params IDomainEvent[] given)
        {
            return n => subscriptions
                .Where(p => p.Key.NotificationContract.Equals(n.Contract())).Select(p => p.Value)
                .Select<Publisher, Func<INotification, NotificationsByPublisher>>
                (
                    function =>
                        notification =>
                            function(
                                notification,
                                NotificationsByCorrelations(given),
                                () => DateTimeOffset.Now)
                );

        }

        public static Lazy<IEnumerable<NotificationsByPublisher>> Notify<TNotification>(this
            Func<INotification, IEnumerable<Func<INotification, NotificationsByPublisher>>> publishers,
            TNotification notification)
            where TNotification : INotification
        {
            return new Lazy<IEnumerable<NotificationsByPublisher>>(() => publishers(notification)
                .Select(x => x(notification)));
        }

        static NotificationsByCorrelations NotificationsByCorrelations(params IDomainEvent[] notifications)
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
            .Where(n => correlations.Any(c => c.Contract.Value.Equals(n.Notification.Contract.Value)))
            .Where(n => correlations.Any(c => n.Correlations.Any(nc => nc.Contract.Value.Equals(c.Contract.Value) && nc.PropertyName.Equals(c.PropertyName) && nc.PropertyValue.Value.Equals(c.PropertyValue.Value))))
            .Select(x => x.Notification);
        }
    }
}