using System.Collections.Generic;
using System.Linq;
using Hydra.Core;

namespace RetailDomain
{
    public class JustSpinningMyWheels : IDomainEvent
    {
        public IEnumerable<KeyValuePair<string, object>> Correlations => Enumerable.Empty<KeyValuePair<string, object>>();
    }
}