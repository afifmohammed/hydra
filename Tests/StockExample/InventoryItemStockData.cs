using System;
using System.Collections.Generic;
using EventSourcing;

namespace Tests
{
    public struct InventoryItemStockData
    {
        public string Sku { get; set; }
        public int Count { get; set; }
        public bool IsActive { get; set; }
        public int OverStockLimit { get; set; }
    }

    public static class InventoryItemStockHandler
    {
        public static IEnumerable<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>> Publishers()
        {
            return new UseCase<InventoryItemStockData>()
                .Given<InventoryItemCreated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<InventoryItemDeactivated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<ItemsRemovedFromInventory>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<ItemsCheckedInToInventory>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<InventoryItemStockLimitChanged>(Map)
                    .Correlate(x =>x.Id, x => x.Sku)
                .When<InventoryItemStockLimitChangeRequested>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<CheckInItemsRequested>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<RemoveInventoryItemsRequested>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<CreateInventoryItemRequested>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .When<DeactivateInventoryItemRequested>()
                    .Correlate(x => x.Id, x => x.Sku)
                    .Then(Handle)
                .Publishers;
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, InventoryItemStockLimitChangeRequested e)
        {
            if (!d.IsActive)
                return new[] { new InventoryItemActionInvalid { Action = "CheckIn", Id = e.Id, Reason = "ItemInActive" } };

            return new[] { new InventoryItemStockLimitChanged { Id = e.Id, Limit = e.Limit } };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, CheckInItemsRequested e)
        {
            if (!d.IsActive)
                return new [] {new InventoryItemActionInvalid {Action = "CheckIn", Id = e.Id, Reason = "ItemInActive"}};

            if (d.Count > d.OverStockLimit)
                return new[] { new InventoryItemActionInvalid { Action = "CheckIn", Id = e.Id, Reason = "OverStocked" } };
            
            return new[] { new ItemsCheckedInToInventory {Id = e.Id, Count = e.Count} };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, RemoveInventoryItemsRequested e)
        {
            if (!d.IsActive)
                return new[] { new InventoryItemActionInvalid { Action = "CheckOut", Id = e.Id, Reason = "ItemInActive" } };

            if (d.Count < e.Count)
                return new[] { new InventoryItemActionInvalid { Action = "CheckOut", Id = e.Id, Reason = "BelowZeroStock" } };

            return new[] { new ItemsRemovedFromInventory { Id = e.Id, Count = e.Count } };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, DeactivateInventoryItemRequested e)
        {
            if (!d.IsActive)
                return new[] { new JustSpinningMyWheels() };

            return new[] { new InventoryItemDeactivated  { Id = e.Id } };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, CreateInventoryItemRequested e)
        {
            if (d.Sku.Equals(e.Id))
                return new[] { new JustSpinningMyWheels() };

            return new[] { new InventoryItemCreated { Id = e.Id } };
        }

        public static InventoryItemStockData Map(ItemsCheckedInToInventory e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                Sku = e.Id,
                OverStockLimit = d.OverStockLimit,
                Count = d.Count + e.Count
            };
        }

        public static InventoryItemStockData Map(ItemsRemovedFromInventory e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                Sku = e.Id,
                OverStockLimit = d.OverStockLimit,
                Count = d.Count - e.Count
            };
        }

        public static InventoryItemStockData Map(InventoryItemDeactivated e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                Sku = e.Id,
                OverStockLimit = d.OverStockLimit,
                IsActive = false
            };
        }

        public static InventoryItemStockData Map(InventoryItemCreated e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                Sku = e.Id
            };
        }

        private static InventoryItemStockData Map(InventoryItemStockLimitChanged e, InventoryItemStockData d)
        {
            d.OverStockLimit = e.Limit;
            return d;
        }
    }
}