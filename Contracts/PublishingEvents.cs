using System;
using System.Collections.Generic;

namespace Hydra.Core
{
    public delegate Action<IEnumerable<INotification>> Notify(Func<IEnumerable<Subscription>> subscriptions);

    public delegate void Post(IEnumerable<SubscriberMessage> messages);

    public delegate void Enqueue<in TProvider>(
        TProvider provider,
        IEnumerable<SubscriberMessage> messages)
        where TProvider : IProvider;
}