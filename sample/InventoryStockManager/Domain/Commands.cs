using System.Collections.Generic;
using Commands;
using EventSourcing;

namespace InventoryStockManager.Domain
{
    public class ChangeInventoryItemStockLimit : ICommand
    {
        public string Id { get; set; }
        public int Limit { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class CreateInventoryItem : ICommand
    {
        public string Id { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class DeactivateInventoryItem : ICommand
    {
        public string Id { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class CheckInItems : ICommand
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class RemoveInventoryItems : ICommand
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }
}