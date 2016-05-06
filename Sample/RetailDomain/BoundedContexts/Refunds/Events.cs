using System;
using System.Collections.Generic;
using EventSourcing;

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

    public class CustomerMarkedAsFraud : IDomainEvent
    {
        public string CustomerId { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.CustomerId)
        };
    }
}