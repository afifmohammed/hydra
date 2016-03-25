using System.Collections.Generic;
using Xunit;

namespace EventSourcing
{
    class CustomerRegistered : IDomainEvent
    {
        public string CustomerId { get; set; }
    }

    class OrderPlaced : IDomainEvent
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Sku { get; set; }        
    }

    class StockReserved : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }        
    }

    class OrderOutofStock : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }
    }

    class OrderCancelled : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }        
    }

    public struct ReserveStock
    {
        public string Sku { get; set; }
        public int Available { get; set; }
    }

    static class ReserveStockHandler
    {      
        public static IEnumerable<IDomainEvent> On(ReserveStock data, OrderPlaced e)
        {
            return data.Available >= 1 
                ? new IDomainEvent[] { new StockReserved { OrderId = e.OrderId, Sku = data.Sku } }
                : new IDomainEvent[] { new OrderOutofStock { OrderId = e.OrderId, Sku = data.Sku } };
        }
    }

    class Tests
    {
        [Fact]
        public void WorksOutOfTheBox()
        {
            /*
            Functions.FoldHandlerData<ReserveStock, OrderPlaced>
            (
                new OrderPlaced { OrderId = "1", Sku = "909", CustomerId = "333" },
                new [] 
                {
                    CorrelationMap.For<ReserveStock, OrderPlaced>(x => x.Sku, x => x.Sku)
                },

            );
            */
        }
    }
}
