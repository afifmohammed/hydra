using System.Collections.Generic;
using EventSourcing;
using RequestPipeline;

namespace RetailDomain.Inventory
{
    public class ChangeInventoryItemStockLimit : IRequest<Unit>, ICorrelated
    {
        public string Id { get; set; }
        public int Limit { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class CreateInventoryItem : IRequest<Unit>, ICorrelated
    {
        public string Id { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class DeactivateInventoryItem : IRequest<Unit>, ICorrelated
    {
        public string Id { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class CheckInItems : IRequest<Unit>, ICorrelated
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }

    public class RemoveInventoryItems : IRequest<Unit>, ICorrelated
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.Id) };
    }
}