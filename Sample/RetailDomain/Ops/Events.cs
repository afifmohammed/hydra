using System;
using System.Collections.Generic;
using EventSourcing;

namespace RetailDomain.Ops
{
    public class OrderPlaced : IDomainEvent
    {
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public string CustomerId { get; set; }
        public DateTimeOffset? When { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.OrderId),
            this.PropertyNameValue(x => x.CustomerId),
            this.PropertyNameValue(x => x.ProductId)
        };
    }
}