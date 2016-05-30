﻿using Hydra.AdoNet;
using Hydra.Core.FluentInterfaces;
using RetailDomain.Inventory;
using DapperExtensions;
namespace RetailViews
{
    public class InventoryItemStockView
    {
        public string Sku { get; set; }
        public int Count { get; set; }
        public bool? IsActive { get; set; }
        public int OverStockLimit { get; set; }
    }

    public static class InventoryItemStockViewBuilder<TConnectionStringName> where TConnectionStringName : class
    {
        public static ConsumerContractSubscriptions<InventoryItemStockView, AdoNetTransactionUowProvider<TConnectionStringName>> Subscriptions()
        {
            return new Denormalizer<InventoryItemStockView, TConnectionStringName>()
                .When<InventoryItemCreated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<InventoryItemDeactivated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<InventoryItemStockLimitChanged>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<ItemsCheckedInToInventory>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<ItemsRemovedFromInventory>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle);

        }

        private static void Handle(InventoryItemStockView view, ItemsRemovedFromInventory notification, AdoNetTransactionUowProvider<TConnectionStringName> uowProvider)
        {
            uowProvider.Value.Connection.Update(Map(notification, view), uowProvider.Value);
        }

        private static void Handle(InventoryItemStockView view, ItemsCheckedInToInventory notification, AdoNetTransactionUowProvider<TConnectionStringName> uowProvider)
        {
            uowProvider.Value.Connection.Update(Map(notification, view), uowProvider.Value);
        }

        private static void Handle(InventoryItemStockView view, InventoryItemStockLimitChanged notification, AdoNetTransactionUowProvider<TConnectionStringName> uowProvider)
        {
            uowProvider.Value.Connection.Update(Map(notification, view), uowProvider.Value);
        }

        private static void Handle(InventoryItemStockView view, InventoryItemDeactivated notification, AdoNetTransactionUowProvider<TConnectionStringName> uowProvider)
        {
            uowProvider.Value.Connection.Update(Map(notification, view), uowProvider.Value);
        }

        private static void Handle(InventoryItemStockView view, InventoryItemCreated notification, AdoNetTransactionUowProvider<TConnectionStringName> uowProvider)
        {
            uowProvider.Value.Connection.Update(Map(notification, view), uowProvider.Value);
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