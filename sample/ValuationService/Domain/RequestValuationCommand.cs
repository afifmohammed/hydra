using System;
using System.Collections.Generic;
using Commands;
using EventSourcing;

namespace ValuationService.Domain
{
    public class RequestValuationCommand : ICommand
    {
        public int CustomerId { get; set; }
        public Guid LoanId { get; set; }
        public ValuationTypes ValuationType { get; set; }
        public ValuationReasons ValuationReason { get; set; }
        public string PropertyType { get; set; }
        public EstimatedMarketTypes EstimatedMarketType { get; set; }
        public string Notes { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.LoanId),
            this.PropertyNameValue(x => x.CustomerId)
        };
    }
}