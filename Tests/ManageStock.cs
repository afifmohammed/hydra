using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Core;
using Hydra.RequestPipeline;
using RetailDomain.Inventory;
using Xunit;

namespace Tests
{
    public class CanViewAStockedItem
    {
        [Fact]
        public void WhenCreatedAndStocked()
        {
            var store = new InMemoryView<InventoryItemStockView>();
            InventoryItemStockViewBuilder.Subscriptions()
                .ExportersBySubscription
                .Given(
                    new InventoryItemCreated { Id = "1" },
                    new ItemsCheckedInToInventory { Id = "1", Count = 10 },
                    new ItemsCheckedInToInventory { Id = "1", Count = 30 },
                    new InventoryItemCreated { Id = "2" },
                    new ItemsCheckedInToInventory { Id = "2", Count = 3 },
                    new ItemsCheckedInToInventory { Id = "1", Count = 50 })
                .Notify(new ItemsCheckedInToInventory { Id = "1", Count = 7 }, store);
            
            Assert.Equal(97, store["1"].Count);
        }
    }

    public class CanDeactivateAStockedItem
    {
        readonly Lazy<IEnumerable<NotificationsByPublisher>> _notificationsByPublisher;

        public CanDeactivateAStockedItem()
        {
            _notificationsByPublisher = InventoryItemStockHandler
                .Subscriptions()
                .PublisherBySubscription
                .Given(
                    new InventoryItemCreated {Id = "1"},
                    new ItemsCheckedInToInventory {Id = "1", Count = 10})
                .Notify(new Placed<DeactivateInventoryItem> {Command = new DeactivateInventoryItem {Id = "1"}});
        }

        [Fact]
        public void PublisherHasCorrectNumberOfCorrelations()
        {
            Assert.Equal(1, _notificationsByPublisher.Value.SelectMany(x => x.PublisherDataCorrelations).Count());
        }

        [Fact]
        public void PublisherHasTheCorrectCorrelationContract()
        {
            Assert.Equal(new[] { typeof(InventoryItemStockData).Contract().Value }, _notificationsByPublisher.Value.SelectMany(x => x.PublisherDataCorrelations).Select(x => x.Contract.Value).ToArray());
        }

        [Fact]
        public void PublisherHasTheCorrectCorrelationValue()
        {
            Assert.Equal(new[] { "1" }, _notificationsByPublisher.Value.SelectMany(x => x.PublisherDataCorrelations).Select(x => x.PropertyValue.Value).ToArray());
        }

        [Fact]
        public void PublisherHasPublishedTheRightNotifications()
        {
            var notifications = _notificationsByPublisher
                .Value
                .SelectMany(n => n.Notifications)
                .Select(n => n.Item1)
                .ToList();

            Assert.Equal(1, notifications.Count);
            Assert.Equal(typeof(InventoryItemDeactivated).Contract().Value, notifications.Select(n => n.Contract().Value).Single());
            Assert.Equal("1", notifications.Cast<InventoryItemDeactivated>().Single().Id);
        }
    }
}