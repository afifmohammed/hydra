using System.Collections.Generic;
using Commands;
using EventSourcing;

namespace ValuationService.Domain
{
    public class UpdateCustomerCommand : ICommand
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.CustomerId),
            this.PropertyNameValue(x=> x.CustomerName)
        };
    }
}