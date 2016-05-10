using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using RequestPipeline;
using Xunit;
using RetailDomain.Refunds;
using RetailDomain.Risk;
using RetailDomain.Sales;
using RetailDomain.Ops;

namespace Tests
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
                    new CustomerMarkedAsFraud {CustomerId = "customers/1"},
                    new ProductOnSale {ProductId = "skus/1", PolicyId = "policy/1", WhenSaleExpires = DateTimeOffset.Now.AddDays(10)},
                    new PolicyInPlace { PolicyId = "policy/1", RefundsAllowed = true, CoolingOffPeriodInDays = 10},
                    new OrderPlaced { OrderId = "orders/1", ProductId = "skus/1", CustomerId = "customers/1", When = DateTimeOffset.Now.AddDays(-3) }
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

    public class CannotRefundWhenCannotFindProductOnSale
    {
        readonly Lazy<IEnumerable<NotificationsByPublisher>> _notificationsByPublisher;

        public CannotRefundWhenCannotFindProductOnSale()
        {
            _notificationsByPublisher = RefundProductOrderHandler
                .Subscriptions()
                .PublisherBySubscription
                .Given
                (
                    new PolicyInPlace { PolicyId = "policy/1", RefundsAllowed = true, CoolingOffPeriodInDays = 10},
                    new OrderPlaced { OrderId = "orders/1", ProductId = "skus/1", CustomerId = "customers/1" }
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

    public class CannotRefundWhenCannotFindPolicy
    {
        readonly Lazy<IEnumerable<NotificationsByPublisher>> _notificationsByPublisher;

        public CannotRefundWhenCannotFindPolicy()
        {
            _notificationsByPublisher = RefundProductOrderHandler
                .Subscriptions()
                .PublisherBySubscription
                .Given
                (
                    new ProductOnSale { PolicyId = "policy/1", ProductId = "skus/1", WhenSaleExpires = DateTimeOffset.Now.AddDays(10)},
                    new OrderPlaced { OrderId = "orders/1", ProductId = "skus/1", CustomerId = "customers/1" }
                )
                .Notify(new Placed<RefundProductOrder> { Command = new RefundProductOrder { OrderId = "orders/1" } });
        }

        [Fact]
        public void PublisherThrowsAnExceptionWhenDataNotAvailable()
        {
            Assert.Throws<CannotFindProductPolicy>(() => _notificationsByPublisher
                .Value
                .SelectMany(n => n.Notifications)
                .Select(n => n.Item1)
                .ToList());
        }
    }

    
}