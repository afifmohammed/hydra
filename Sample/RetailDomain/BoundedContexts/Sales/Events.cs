using System;
using System.Collections.Generic;
using Hydra.Core;

namespace RetailDomain.Sales
{
    public class ProductOnSale : IDomainEvent
    {
        public string ProductId { get; set; }
        public string PolicyId { get; set; }
        public DateTimeOffset WhenSaleExpires { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.ProductId),
            this.PropertyNameValue(x => x.PolicyId)
        };
    }

    public class PolicyInPlace : IDomainEvent
    {
        public string PolicyId { get; set; }
        public bool RefundsAllowed { get; set; }
        public int CoolingOffPeriodInDays { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.PolicyId)
        };
    }
}