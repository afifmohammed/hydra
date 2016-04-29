using System;

namespace ValuationService.Domain
{
    public struct Valuation
    {
        public int CustomerId { get; set; }
        public Guid LoanId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public ValuationTypes ValuationType { get; set; }
        public ValuationReasons ValuationReason { get; set; }
        public string PropertyType { get; set; }
        public EstimatedMarketTypes EstimatedMarketType { get; set; }
        public string Notes { get; set; }

        public bool IsPopulated()
        {
            return LoanId != Guid.Empty && !string.IsNullOrWhiteSpace(CustomerName);
        }
    }
}