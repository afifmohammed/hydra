using System.Collections.Generic;

namespace EventSourcing
{
    public interface IQuery { }

    public delegate IEnumerable<object> Query(IQuery query);
}