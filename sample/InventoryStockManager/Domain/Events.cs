using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace InventoryStockManager.Domain
{
    public class InventoryItemStockLimitChanged : IDomainEvent
    {
        public string Id { get; set; }
        public int Limit { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class InventoryItemCreated : IDomainEvent
    {
        public string Id { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class InventoryItemDeactivated : IDomainEvent
    {
        public string Id { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class ItemsCheckedInToInventory : IDomainEvent
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class ItemsRemovedFromInventory : IDomainEvent
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class InventoryItemActionInvalid : IDomainEvent
    {
        public string Id { get; set; }
        public string Action { get; set; }
        public string Reason { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class JustSpinningMyWheels : IDomainEvent
    {
        public IEnumerable<KeyValuePair<string, object>> Correlations => Enumerable.Empty<KeyValuePair<string, object>>();
    }
}