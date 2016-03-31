using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace Tests
{
    static class Extensions
    {
        public static IEnumerable<Func<IDomainEvent, NotificationsByPublisher>> Given<TNotification>(
            this Subsriptions subscriptions,
            params IDomainEvent[] given)
            where TNotification : IDomainEvent
        {
            var publishers = subscriptions.PublisherByNotificationAndPublisherContract.Where(p => p.Key.Item1.Equals(typeof(TNotification).Contract())).Select(p => p.Value);

            return publishers.Select<
                    Func<IDomainEvent, Func<IEnumerable<Correlation>, IEnumerable<SerializedNotification>>, Func<DateTimeOffset>, NotificationsByPublisher>, 
                    Func<IDomainEvent, NotificationsByPublisher>>
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
            IEnumerable<Func<IDomainEvent, NotificationsByPublisher>> publishers,
            TNotification notification)
            where TNotification : IDomainEvent
        {
            return publishers
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