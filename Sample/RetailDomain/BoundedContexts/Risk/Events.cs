using System.Collections.Generic;
using EventSourcing;

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