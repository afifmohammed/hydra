using System.Collections.Generic;
using Hydra.Core;

namespace RetailDomain.Risk
{
    public class CustomerMarkedAsFraud : IDomainEvent
    {
        public string CustomerId { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.CustomerId)
        };
    }
}