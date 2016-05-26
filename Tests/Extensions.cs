using System;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hydra.Core;
using Hydra.Core.FluentInterfaces;

namespace Tests
{
    static class Extensions
    {
        public static Func<TypeContract, IEnumerable<Action<INotification, TProvider>>> Given<TProvider>(
            this ExportersBySubscription<TProvider> subscriptions,
            params IDomainEvent[] given)
            where TProvider : IProvider
        {
            return n => subscriptions
                .Where(p => p.Key.NotificationContract.Equals(n))
                .Select(p => p.Value)
                .Select<Exporter<TProvider>, Action<INotification, TProvider>>(exporter =>
                    (notification, provider) =>
                            exporter(
                                notification,
                                NotificationsByCorrelations(given),
                                () => DateTimeOffset.Now,
                                provider));
        }

        public static void Notify<TProvider>(
            this Func<TypeContract, IEnumerable<Action<INotification, TProvider>>> consumerContractSubscriptions,
            IDomainEvent notification,
            TProvider provider)
            where TProvider : IProvider
        {
            foreach (var action in consumerContractSubscriptions(notification.Contract()))
            {
                action(notification, provider);
            }
        }

        public static Func<TypeContract, IEnumerable<Func<INotification, NotificationsByPublisher>>> Given(
            this PublishersBySubscription subscriptions,
            params IDomainEvent[] given)
        {
            return contract => subscriptions
                .Where(p => p.Key.NotificationContract.Equals(contract))
                .Select(p => p.Value)
                .Select<Publisher, Func<INotification, NotificationsByPublisher>>
                (
                    publisher =>
                        notification =>
                            publisher(
                                notification,
                                NotificationsByCorrelations(given),
                                () => DateTimeOffset.Now)
                );

        }

        public static ConsumerContractSubscriptions<TSubscriberDataContract, TProvider> Notify<TSubscriberDataContract, TProvider>(
            this ConsumerContractSubscriptions<TSubscriberDataContract, TProvider> consumerContractSubscriptions,
            IDomainEvent notification,
            TProvider provider)
            where TSubscriberDataContract : new()
            where TProvider : IProvider
        {
            var subscription = new Subscription(
                notificationContract: new TypeContract(notification),
                subscriberDataContract: new TypeContract(typeof(TSubscriberDataContract)));

            consumerContractSubscriptions
                .ExportersBySubscription[subscription]
                (
                    notification,
                    NotificationsByCorrelations(),
                    () => DateTimeOffset.Now,
                    provider
                );
            return consumerContractSubscriptions;
        }

        public static Lazy<IEnumerable<NotificationsByPublisher>> Notify<TNotification>(
            this Func<TypeContract, IEnumerable<Func<INotification, NotificationsByPublisher>>> publishers,
            TNotification notification)
            where TNotification : INotification
        {
            return new Lazy<IEnumerable<NotificationsByPublisher>>(() => publishers(notification.Contract())
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