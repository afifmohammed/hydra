using System.Collections.Generic;
using EventSourcing;
using RequestPipeline;

namespace RetailDomain.Refunds
{
    public class RefundProductOrder : IRequest<Unit>, ICorrelated
    {
        public string OrderId { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Correlations => new[]
        {
            this.PropertyNameValue(x => x.OrderId),
        };
    }
}