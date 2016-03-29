using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using Xunit;

namespace Tests
{
    public class Tests
    {
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
                .Where(n => correlations.All(c => n.Correlations.Any(nc => nc.Equals(c))))
                .Select(x => x.Notification);
        }

        [Fact]
        public void CanDeactivateAStockedItem()
        {
            GlobalConfiguration.NotificationsByCorrelations = NotificationsByCorrelations
            (
                new InventoryItemCreated {Id = "1"},
                new ItemsCheckedInToInventory { Id = "1", Count = 10 }
            );

            var publishers = InventoryItemStockHandler.Publishers().ToList();

            var notifications = publishers.Notify(new DeactivateInventoryItemRequested {Id = "1"}).ToList();

            Assert.NotEmpty(notifications);
            Assert.True(notifications.SelectMany(n => n.Notifications).Select(n => n.Item1).Any(n => n is InventoryItemDeactivated));
        }
    }

    static class Extensions
    {
        public static IEnumerable<NotificationsByPublisher> Notify<TNotification>(
            this IEnumerable<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>> publishers,
            TNotification notification)
            where TNotification : IDomainEvent
        {
            return publishers
                .Where(x => x.Key.Equals(new TypeContract(typeof (TNotification))))
                .Select(x => x.Value(notification));
        }
    }
}