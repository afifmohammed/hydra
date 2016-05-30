using System;
using System.Collections.Generic;

namespace Hydra.Core
{
    public delegate Action<IEnumerable<Event>> Notify(Func<IEnumerable<Subscription>> subscriptions);

    public delegate void Post(IEnumerable<SubscriberMessage> messages);

    public delegate void Enqueue<in TUowProvider>(
        TUowProvider provider,
        IEnumerable<SubscriberMessage> messages)
        where TUowProvider : IUowProvider;
}