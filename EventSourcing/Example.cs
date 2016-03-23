using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    class OrderPlaced : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }
        public int Qty { get; set; }
        public DateTimeOffset When { get; set; }
    }

    class StockReserved : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }
        public int Qty { get; set; }
        public DateTimeOffset When { get; set; }
    }

    class OrderCancelled : IDomainEvent
    {
        public string OrderId { get; set; }
        public string Sku { get; set; }
        public int Qty { get; set; }
        public DateTimeOffset When { get; set; }
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
            return Enumerable.Empty<IDomainEvent>();
        }
    }
}
