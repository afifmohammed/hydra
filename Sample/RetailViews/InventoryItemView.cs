using Hydra.AdoNet;
using Hydra.Core.FluentInterfaces;
using RetailDomain.Inventory;

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
        public static ConsumerContractSubscriptions<InventoryItemStockView, AdoNetTransactionProvider<TConnectionStringName>> Subscriptions()
        {
            return new Denormalizer<InventoryItemStockView, TConnectionStringName>()
                .When<InventoryItemCreated>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<InventoryItemDeactivated>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<InventoryItemStockLimitChanged>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<ItemsCheckedInToInventory>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<ItemsRemovedFromInventory>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle);

        }

        private static void Handle(InventoryItemStockView arg1, ItemsRemovedFromInventory arg2, AdoNetTransactionProvider<TConnectionStringName> arg3)
        {
            
        }

        private static void Handle(InventoryItemStockView arg1, ItemsCheckedInToInventory arg2, AdoNetTransactionProvider<TConnectionStringName> arg3)
        {
            
        }

        private static void Handle(InventoryItemStockView arg1, InventoryItemStockLimitChanged arg2, AdoNetTransactionProvider<TConnectionStringName> arg3)
        {
            
        }

        private static void Handle(InventoryItemStockView arg1, InventoryItemDeactivated arg2, AdoNetTransactionProvider<TConnectionStringName> arg3)
        {
            
        }

        private static void Handle(InventoryItemStockView arg1, InventoryItemCreated inventoryItemCreated, AdoNetTransactionProvider<TConnectionStringName> arg3)
        {
            
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