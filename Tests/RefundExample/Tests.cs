using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using RequestPipeline;
using Xunit;

namespace Tests.RefundExample
{
    public class CannotRefundWhenCustomerIsFraud
    {
        readonly Lazy<IEnumerable<NotificationsByPublisher>> _notificationsByPublisher;

        public CannotRefundWhenCustomerIsFraud()
        {
            _notificationsByPublisher = RefundProductOrderHandler
                .Subscriptions()
                .PublisherBySubscription
                .Given
                (
                    new CustomerMarkedAsFraud {CustomerId = "customers/4"},
                    new ProductOnSale {ProductId = "skus/2", WhenSaleExpires = DateTimeOffset.Now.AddDays(10)},
                    new PolicyInPlace { PolicyId = "skus/2", RefundsAllowed = true, CoolingOffPeriodInDays = 10},
                    new OrderPlaced { OrderId = "orders/1", ProductId = "skus/2", CustomerId = "customers/4", When = DateTimeOffset.Now.AddDays(-3) }
                )
                .Notify(new Placed<RefundProductOrder> {Command = new RefundProductOrder {OrderId = "orders/1"}, When = DateTimeOffset.Now.AddDays(-1)});
        }

        [Fact]
        public void PublisherHasPublishedTheRightNotifications()
        {
            var notifications = _notificationsByPublisher
                .Value
                .SelectMany(n => n.Notifications)
                .Select(n => n.Item1)
                .ToList();

            Assert.Equal(1, notifications.Count);
            Assert.Equal(typeof(RefundRejected).Contract().Value, notifications.Select(n => n.Contract().Value).Single());
            Assert.Equal("orders/1", notifications.Cast<RefundRejected>().Single().OrderId);
        }
    }

    public class CannotRefundWhenCannotDetermineProductOnSale
    {
        readonly Lazy<IEnumerable<NotificationsByPublisher>> _notificationsByPublisher;

        public CannotRefundWhenCannotDetermineProductOnSale()
        {
            _notificationsByPublisher = RefundProductOrderHandler
                .Subscriptions()
                .PublisherBySubscription
                .Given
                (
                    new PolicyInPlace { PolicyId = "skus/2", RefundsAllowed = true, CoolingOffPeriodInDays = 10},
                    new OrderPlaced { OrderId = "orders/1", ProductId = "skus/2", CustomerId = "customers/4" }
                )
                .Notify(new Placed<RefundProductOrder> { Command = new RefundProductOrder { OrderId = "orders/1" } });
        }

        [Fact]
        public void PublisherThrowsAnExceptionWhenDataNotAvailable()
        {
            Assert.Throws<CannotFindProductOnSale>(() => _notificationsByPublisher
                .Value
                .SelectMany(n => n.Notifications)
                .Select(n => n.Item1)
                .ToList());
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

    public class RefundProductOrder : IRequest<Unit>, ICorrelated
    {
        public string OrderId { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.OrderId),
        };
    }

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
                    .Correlate(x => x.CustomerId, x => x.Order.Sku)        
                .When<Placed<RefundProductOrder>>()
                    .Correlate(x => x.Command.OrderId, x => x.OrderId)
                    .Then(Handle);
        }

        private static RefundProductOrderData Map(PolicyInPlace e, RefundProductOrderData d)
        {
            return d.With(x => x.Policy = new ProductPolicy {CoolingOffPeriodInDays = e.CoolingOffPeriodInDays, RefundAllowed = e.RefundsAllowed});
        }

        private static RefundProductOrderData Map(ProductOnSale e, RefundProductOrderData d)
        {
            return d.With(x => x.Product = new Product {PolicyId = e.PolicyId, WhenSaleExpires = e.WhenSaleExpires});
        }

        private static RefundProductOrderData Map(CustomerMarkedAsFraud e, RefundProductOrderData d)
        {
            return d.With(x => x.Customer = new Customer {CustomerMarkedAsFraud = true});
        }

        private static RefundProductOrderData Map(OrderPlaced e, RefundProductOrderData d)
        {
            return d.With(x =>
            {
                x.OrderId = e.OrderId;
                x.Order = new Order {CustomerId = e.CustomerId, Sku = e.ProductId, WhenOrderPlaced = e.When};
            });
        }

        private static IEnumerable<IDomainEvent> Handle(RefundProductOrderData d, Placed<RefundProductOrder> e)
        {
            if (d.Product == null)
                throw new CannotFindProductOnSale();

            if (d.Policy == null)
                throw new CannotFindProductPolicy();

            if (d.Customer?.CustomerMarkedAsFraud ?? false)
                return new[] {new RefundRejected {OrderId = d.OrderId} };

            if (e.When.Value > d.Product.WhenSaleExpires)
                return new[] { new RefundRejected { OrderId = d.OrderId } };

            if (e.When.Value.Subtract(d.Order.WhenOrderPlaced.Value).TotalDays > d.Policy.CoolingOffPeriodInDays.Value)
                return new[] { new RefundRejected { OrderId = d.OrderId } };

            return new[] { new RefundApproved { OrderId = d.OrderId } };
        }
    }

    class CannotFindProductPolicy : EventualConsistencyException { }
    class CannotFindProductOnSale : EventualConsistencyException { }
    class CannotFindOrderPlaced : EventualConsistencyException { }
    class EventualConsistencyException : Exception {}
}