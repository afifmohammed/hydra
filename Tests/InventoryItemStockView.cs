using System.Collections.Generic;
using EventSourcing;
using RetailDomain.Inventory;

namespace Tests
{
    public class Denormalizer<TView> : ConsumerBuilder<TView, Dictionary<string, TView>> where TView : new()
    { }

    public class InventoryItemStockView
    {
        public string Sku { get; set; }
        public int Count { get; set; }
        public bool? IsActive { get; set; }
        public int OverStockLimit { get; set; }
    }

    public static class InventoryItemStockViewBuilder
    {
        public static ConsumerContractSubscriptions<InventoryItemStockView, Dictionary<string, InventoryItemStockView>> Subscriptions()
        {
            return new Denormalizer<InventoryItemStockView>()
                .When<InventoryItemCreated>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<InventoryItemDeactivated>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<InventoryItemStockLimitChanged>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<ItemsCheckedInToInventory>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<ItemsRemovedFromInventory>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view));

        }

        private static InventoryItemStockView Map(ItemsRemovedFromInventory e, InventoryItemStockView v)
        {
            return new InventoryItemStockView
            {
                IsActive = v.IsActive,
                Sku = v.Sku,
                Count = v.Count - e.Count,
                OverStockLimit = v.OverStockLimit
            };
        }
        
        private static InventoryItemStockView Map(ItemsCheckedInToInventory e, InventoryItemStockView v)
        {
            return new InventoryItemStockView
            {
                IsActive = v.IsActive,
                Sku = v.Sku,
                Count = v.Count + e.Count,
                OverStockLimit = v.OverStockLimit
            };
        }

        private static InventoryItemStockView Map(InventoryItemStockLimitChanged e, InventoryItemStockView v)
        {
            return new InventoryItemStockView
            {
                IsActive = v.IsActive,
                Sku = v.Sku,
                Count = v.Count,
                OverStockLimit = e.Limit
            };
        }

        private static InventoryItemStockView Map(InventoryItemDeactivated e, InventoryItemStockView v)
        {
            return new InventoryItemStockView
            {
                IsActive = false,
                Sku = v.Sku,
                Count = v.Count,
                OverStockLimit = v.OverStockLimit
            };
        }

        private static InventoryItemStockView Map(InventoryItemCreated e, InventoryItemStockView v)
        {
            return new InventoryItemStockView
            {
                IsActive = v.IsActive ?? true,
                Sku = e.Id,
                Count = v.Count,
                OverStockLimit = v.OverStockLimit
            };
        }
    }
}