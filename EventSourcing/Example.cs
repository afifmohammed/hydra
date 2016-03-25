using System.Collections.Generic;

namespace EventSourcing
{
    class OrderPlaced : IDomainEvent
    {
        public string OrderId { get; set; }
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

    public struct StockReservationData
    {
        public string Sku { get; set; }
        public int Available { get; set; }
    }

    static class ReserveStock
    {      
        public static IEnumerable<IDomainEvent> On(StockReservationData data, OrderPlaced e)
        {
            return data.Available >= 1 
                ? new IDomainEvent[] { new StockReserved { OrderId = e.OrderId, Sku = data.Sku } }
                : new IDomainEvent[] { new OrderOutofStock { OrderId = e.OrderId, Sku = data.Sku } };
        }
    }
}
