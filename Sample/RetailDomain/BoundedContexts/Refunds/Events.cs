using System;
using System.Collections.Generic;
using Hydra.Core;

namespace RetailDomain.Refunds
{
    public class RefundApproved : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.OrderId),
            this.PropertyNameValue(x => x.Sku)
        };
    }

    public class RefundRejected : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.OrderId),
            this.PropertyNameValue(x => x.Sku)
        };
    }
}