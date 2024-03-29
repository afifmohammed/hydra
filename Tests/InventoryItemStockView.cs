﻿using System.Collections.Generic;
using Hydra.Core;
using Hydra.Core.FluentInterfaces;
using RetailDomain.Inventory;

namespace Tests
{
    public class InMemoryView<TView> : Dictionary<string, TView>, IUowProvider 
        where TView : new()
    { }
    public class Denormalizer<TView> : ConsumerBuilder<TView, InMemoryView<TView>> where TView : new()
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
        public static ConsumerContractSubscriptions<InventoryItemStockView, InMemoryView<InventoryItemStockView>> Subscriptions()
        {
            return new Denormalizer<InventoryItemStockView>()
                .When<InventoryItemCreated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<InventoryItemDeactivated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<InventoryItemStockLimitChanged>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<ItemsCheckedInToInventory>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then((view, notification, store) => store[notification.Id] = Map(notification, view))
                .When<ItemsRemovedFromInventory>(Map)
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