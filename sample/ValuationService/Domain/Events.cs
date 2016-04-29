using System;
using System.Collections.Generic;
using EventSourcing;

namespace ValuationService.Domain
{
    public class ValuationProcessRequestRejectedEvent : IDomainEvent
    {
        public Guid LoanId { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.LoanId) };
    }

    public class ValuationProcessRequestSentEvent : IDomainEvent
    {
        public Guid LoanId { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.LoanId) };
    }

    public class CustomerChangedEvent : IDomainEvent
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[] { this.PropertyNameValue(x => x.CustomerId) };
    }

    public class ValuationRequestedEvent : IDomainEvent
    {
        public int CustomerId { get; set; }
        public Guid LoanId { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.CustomerId),
            this.PropertyNameValue(x=>x.LoanId)
        };
    }
}