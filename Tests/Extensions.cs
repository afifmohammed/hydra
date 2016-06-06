using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;
using Hydra.Core.FluentInterfaces;

namespace Tests
{
    static class Extensions
    {
        public static IEnumerable<Event> AsEvents(this IDomainEvent[] events)
        {
            var id = new EventId();

            return events.Select(domainEvent =>
            {
                id = id.Increment();
                return new Event
                {
                    Notification = domainEvent,
                    EventId = id
                };
            }).ToList();
        }

        public static Func<TypeContract, IEnumerable<Action<INotification, TUowProvider>>> Given<TUowProvider>(
            this ProjectorsBySubscription<TUowProvider> subscriptions,
            params IDomainEvent[] given)
            where TUowProvider : IUowProvider
        {
            var list = given.AsEvents().ToList().AsReadOnly();

            return n => subscriptions
                .Where(p => p.Key.NotificationContract.Equals(n))
                .Select(p => p.Value)
                .Select<Projector<TUowProvider>, Action<INotification, TUowProvider>>(projector =>
                    (notification, provider) =>
                            projector(
                                new Event { Notification = notification, EventId = list.OrderByDescending(g => g.EventId.Value).First().EventId.With(x => x.Increment()) },
                                NotificationsByCorrelations(list),
                                () => DateTimeOffset.Now,
                                provider));
        }

        public static void Notify<TUowProvider>(
            this Func<TypeContract, IEnumerable<Action<INotification, TUowProvider>>> consumerContractSubscriptions,
            IDomainEvent notification,
            TUowProvider provider)
            where TUowProvider : IUowProvider
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
            var list = given.AsEvents().ToList().AsReadOnly();

            return contract => subscriptions
                .Where(p => p.Key.NotificationContract.Equals(contract))
                .Select(p => p.Value)
                .Select<Publisher, Func<INotification, NotificationsByPublisher>>
                (
                    publisher =>
                        notification =>
                            publisher(
                                new Event {Notification = notification, EventId = new EventId {Value = list.OrderByDescending(g => g.EventId.Value).First().EventId.Value + 1} }, 
                                NotificationsByCorrelations(list),
                                () => DateTimeOffset.Now)
                );

        }

        public static ConsumerContractSubscriptions<TSubscriberDataContract, TUowProvider> Notify<TSubscriberDataContract, TUowProvider>(
            this ConsumerContractSubscriptions<TSubscriberDataContract, TUowProvider> consumerContractSubscriptions,
            Event notification,
            TUowProvider provider)
            where TSubscriberDataContract : new()
            where TUowProvider : IUowProvider
        {
            var subscription = new Subscription(
                notificationContract: new TypeContract(notification),
                subscriberDataContract: new TypeContract(typeof(TSubscriberDataContract)));

            consumerContractSubscriptions
                .ProjectorsBySubscription[subscription]
                (
                    notification,
                    NotificationsByCorrelations(new Event[] {}),
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

        static NotificationsByCorrelations NotificationsByCorrelations(IEnumerable<Event> notifications)
        {
            return (correlations, eventId) =>
            {
                var cs = correlations
                    .Select(c => new {Contract = c.Contract.Value, c.PropertyName, PropertyValue = c.PropertyValue.Value})
                    .ToList();

                var one = notifications
                    .Select(@event => new
                    {
                        Notification = new SerializedNotification
                        {
                            Contract = @event.Notification.Contract(),
                            JsonContent = new JsonContent(@event.Notification)
                        },
                        Correlations = @event.Notification.Correlations(),
                        @event.EventId
                    }).ToList();

                one = one
                    .Where(n => cs.Any(c => c.Contract.Equals(n.Notification.Contract.Value)))
                    .ToList();

                var two = one
                    .Where(
                        n =>
                            cs.Any(
                                c =>
                                    n.Correlations.Any(
                                        nc =>
                                            nc.Contract.Value.Equals(c.Contract) &&
                                            nc.PropertyName.Equals(c.PropertyName) &&
                                            nc.PropertyValue.Value.Equals(c.PropertyValue)))).ToList();

                var three = two
                    .Where(n => eventId is NoEventId || n.EventId.Value < eventId.Value)
                    .ToList();

                var list = three.Select(n => n.Notification).ToList();

                return list;
            };
        }

        static EventId Increment(this EventId eventId)
        {
            eventId.Value = eventId.Value + 1;
            return eventId;
        }
    }
}