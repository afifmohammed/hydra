using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using Xunit;

namespace Tests
{
    public class Tests
    {
        public class CanDeactivateAStockedItem
        {
            readonly List<NotificationsByPublisher> _notificationsByPublisher = new List<NotificationsByPublisher>();
             
            public CanDeactivateAStockedItem()
            {
                _notificationsByPublisher.AddRange(InventoryItemStockHandler
                    .Subsriptions()
                    .PublisherBySubscription
                    .Given<DeactivateInventoryItemRequested>(
                        new InventoryItemCreated { Id = "1" },
                        new ItemsCheckedInToInventory { Id = "1", Count = 10 })
                    .Notify(new DeactivateInventoryItemRequested {Id = "1"}));
            }

            [Fact]
            public void PublisherHasCorrectNumberOfCorrelations()
            {
                Assert.Equal(1, _notificationsByPublisher.SelectMany(x => x.PublisherDataCorrelations).Count());
            }

            [Fact]
            public void PublisherHasTheCorrectCorrelationContract()
            {
                Assert.Equal(new [] {typeof(InventoryItemStockData).Contract().Value}, _notificationsByPublisher.SelectMany(x => x.PublisherDataCorrelations).Select(x => x.Contract.Value).ToArray());
            }

            [Fact]
            public void PublisherHasTheCorrectCorrelationValue()
            {
                Assert.Equal(new [] {"1"}, _notificationsByPublisher.SelectMany(x => x.PublisherDataCorrelations).Select(x => x.PropertyValue.Value).ToArray());
            }

            [Fact]
            public void PublisherHasPublishedTheRightNotifications()
            {
                var notifications = _notificationsByPublisher
                    .SelectMany(n => n.Notifications)
                    .Select(n => n.Item1)
                    .ToList();

                Assert.Equal(1, notifications.Count);
                Assert.Equal(typeof(InventoryItemDeactivated).Contract().Value, notifications.Select(n => n.Contract().Value).Single());
                Assert.Equal("1", notifications.Cast<InventoryItemDeactivated>().Single().Id);
            }
        }
    }
}