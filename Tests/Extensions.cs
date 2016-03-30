using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing;

namespace Tests
{
    static class Extensions
    {
        public static IEnumerable<NotificationsByPublisher> Notify<TNotification>(
            this IEnumerable<KeyValuePair<TypeContract, Func<IDomainEvent, NotificationsByPublisher>>> publishers,
            TNotification notification)
            where TNotification : IDomainEvent
        {
            return publishers
                .Where(x => x.Key.Equals(new TypeContract(typeof (TNotification))))
                .Select(x => x.Value(notification));
        }
    }
}