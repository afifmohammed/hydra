﻿using System.Collections.Generic;
using Commands;
using EventSourcing;

namespace InventoryStockManager.Domain
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
        public static PublisherSubscriptions Subsriptions()
        {
            return new PublisherBuilder<InventoryItemStockData>()
                .Given<InventoryItemCreated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<InventoryItemDeactivated>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<ItemsRemovedFromInventory>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<ItemsCheckedInToInventory>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .Given<InventoryItemStockLimitChanged>(Map)
                    .Correlate(x => x.Id, x => x.Sku)
                .When<Received<ChangeInventoryItemStockLimit>>()
                    .Correlate(x => x.Command.Id, x => x.Sku)
                    .Then(Handle)
                .When<Received<CheckInItems>>()
                    .Correlate(x => x.Command.Id, x => x.Sku)
                    .Then(Handle)
                .When<Received<RemoveInventoryItems>>()
                    .Correlate(x => x.Command.Id, x => x.Sku)
                    .Then(Handle)
                .When<Received<CreateInventoryItem>>()
                    .Correlate(x => x.Command.Id, x => x.Sku)
                    .Then(Handle)
                .When<Received<DeactivateInventoryItem>>()
                    .Correlate(x => x.Command.Id, x => x.Sku)
                    .Then(Handle);
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, Received<ChangeInventoryItemStockLimit> e)
        {
            if (!d.IsActive)
                return new[] { new InventoryItemActionInvalid { Action = "CheckIn", Id = e.Command.Id, Reason = "ItemInActive" } };

            return new[] { new InventoryItemStockLimitChanged { Id = e.Command.Id, Limit = e.Command.Limit } };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, Received<CheckInItems> e)
        {
            if (!d.IsActive)
                return new[] { new InventoryItemActionInvalid { Action = "CheckIn", Id = e.Command.Id, Reason = "ItemInActive" } };

            if (d.Count > d.OverStockLimit)
                return new[] { new InventoryItemActionInvalid { Action = "CheckIn", Id = e.Command.Id, Reason = "OverStocked" } };

            return new[] { new ItemsCheckedInToInventory { Id = e.Command.Id, Count = e.Command.Count } };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, Received<RemoveInventoryItems> e)
        {
            if (!d.IsActive)
                return new[] { new InventoryItemActionInvalid { Action = "CheckOut", Id = e.Command.Id, Reason = "ItemInActive" } };

            if (d.Count < e.Command.Count)
                return new[] { new InventoryItemActionInvalid { Action = "CheckOut", Id = e.Command.Id, Reason = "BelowZeroStock" } };

            return new[] { new ItemsRemovedFromInventory { Id = e.Command.Id, Count = e.Command.Count } };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, Received<DeactivateInventoryItem> e)
        {
            if (!d.IsActive)
                return new[] { new JustSpinningMyWheels() };

            return new[] { new InventoryItemDeactivated { Id = e.Command.Id } };
        }

        public static IEnumerable<IDomainEvent> Handle(InventoryItemStockData d, Received<CreateInventoryItem> e)
        {
            if (d.Sku.Equals(e.Command.Id))
                return new[] { new JustSpinningMyWheels() };

            return new[] { new InventoryItemCreated { Id = e.Command.Id } };
        }

        public static InventoryItemStockData Map(ItemsCheckedInToInventory e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                IsActive = d.IsActive,
                Sku = d.Sku,
                Count = d.Count + e.Count,
                OverStockLimit = d.OverStockLimit
            };
        }

        public static InventoryItemStockData Map(ItemsRemovedFromInventory e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                IsActive = d.IsActive,
                Sku = d.Sku,
                Count = d.Count - e.Count,
                OverStockLimit = d.OverStockLimit
            };
        }

        public static InventoryItemStockData Map(InventoryItemDeactivated e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                IsActive = false,
                Sku = d.Sku,
                Count = d.Count,
                OverStockLimit = d.OverStockLimit
            };
        }

        public static InventoryItemStockData Map(InventoryItemCreated e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                IsActive = true,
                Sku = e.Id
            };
        }

        private static InventoryItemStockData Map(InventoryItemStockLimitChanged e, InventoryItemStockData d)
        {
            return new InventoryItemStockData
            {
                IsActive = d.IsActive,
                Sku = d.Sku,
                Count = d.Count,
                OverStockLimit = e.Limit
            };
        }
    }
}