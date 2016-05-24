using System.Collections.Generic;
using Hydra.Core;
using Hydra.RequestPipeline;

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