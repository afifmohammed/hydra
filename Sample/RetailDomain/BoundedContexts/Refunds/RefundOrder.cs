using System;
using System.Collections.Generic;
using EventSourcing;
using RequestPipeline;
using RetailDomain.Ops;
using RetailDomain.Risk;
using RetailDomain.Sales;

namespace RetailDomain.Refunds
{
    public static class RefundProductOrderHandler
    {
        public static PublisherSubscriptions Subscriptions()
        {
            return new PublisherBuilder<RefundProductOrderData>()
                .Given<PolicyInPlace>(Map)
                    .Correlate(x => x.PolicyId, x => x.Product.PolicyId)
                .Given<OrderPlaced>(Map)
                    .Correlate(x => x.OrderId, x => x.OrderId)
                .Given<ProductOnSale>(Map)
                    .Correlate(x => x.ProductId, x => x.Order.Sku)
                .Given<CustomerMarkedAsFraud>(Map)
                    .Correlate(x => x.CustomerId, x => x.Order.CustomerId)
                .When<Placed<RefundProductOrder>>()
                    .Correlate(x => x.Command.OrderId, x => x.OrderId)
                    .Then(Handle);
        }

        private static RefundProductOrderData Map(PolicyInPlace e, RefundProductOrderData d)
        {
            return d.With(x => x.Policy = new ProductPolicy { CoolingOffPeriodInDays = e.CoolingOffPeriodInDays, RefundAllowed = e.RefundsAllowed });
        }

        private static RefundProductOrderData Map(ProductOnSale e, RefundProductOrderData d)
        {
            return d.With(x => x.Product = new Product { PolicyId = e.PolicyId, WhenSaleExpires = e.WhenSaleExpires });
        }

        private static RefundProductOrderData Map(CustomerMarkedAsFraud e, RefundProductOrderData d)
        {
            return d.With(x => x.Customer = new Customer { CustomerMarkedAsFraud = true });
        }

        private static RefundProductOrderData Map(OrderPlaced e, RefundProductOrderData d)
        {
            return d.With(x =>
            {
                x.OrderId = e.OrderId;
                x.Order = new Order { CustomerId = e.CustomerId, Sku = e.ProductId, WhenOrderPlaced = e.When };
            });
        }

        private static IEnumerable<IDomainEvent> Handle(RefundProductOrderData d, Placed<RefundProductOrder> e)
        {
            if (d.Product == null)
                throw new CannotFindProductOnSale();

            if (d.Policy == null)
                throw new CannotFindProductPolicy();

            if (d.Customer?.CustomerMarkedAsFraud ?? false)
                return new[] { new RefundRejected { OrderId = d.OrderId } };

            if (e.When.Value > d.Product.WhenSaleExpires)
                return new[] { new RefundRejected { OrderId = d.OrderId } };

            if (e.When.Value.Subtract(d.Order.WhenOrderPlaced.Value).TotalDays > d.Policy.CoolingOffPeriodInDays.Value)
                return new[] { new RefundRejected { OrderId = d.OrderId } };

            return new[] { new RefundApproved { OrderId = d.OrderId } };
        }
    }

    public class RefundProductOrderData
    {
        public string OrderId { get; set; }

        public Customer Customer { get; set; }
        public ProductPolicy Policy { get; set; }
        public Product Product { get; set; }
        public Order Order { get; set; }
    }

    public class Order
    {
        public string Sku { get; set; }
        public string CustomerId { get; set; }
        public DateTimeOffset? WhenOrderPlaced { get; set; }
    }

    public class Customer
    {
        public bool? CustomerMarkedAsFraud { get; set; }
    }

    public class Product
    {
        public string PolicyId { get; set; }
        public DateTimeOffset? WhenSaleExpires { get; set; }
    }

    public class ProductPolicy
    {
        public int? CoolingOffPeriodInDays { get; set; }
        public bool? RefundAllowed { get; set; }
    }

    public class CannotFindProductPolicy : EventualConsistencyException { }
    public class CannotFindProductOnSale : EventualConsistencyException { }
    public class CannotFindOrderPlaced : EventualConsistencyException { }
    public class EventualConsistencyException : Exception { }
}