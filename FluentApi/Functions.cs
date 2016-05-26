using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace Hydra.Core
{
    public static class Type<TSource> where TSource : new()
    {
        public static KeyValuePair<TypeContract, CorrelationMap> Correlates<TContract>(
            Expression<Func<TSource, object>> handlerDataProperty,
            Expression<Func<TContract, object>> notificationProperty)
        {
            return new KeyValuePair<TypeContract, CorrelationMap>
            (
                typeof(TSource).Contract(),
                new CorrelationMap
                {
                    HandlerDataContract = typeof(TSource).Contract(),
                    NotificationContract = typeof(TContract).Contract(),
                    NotificationPropertyName = notificationProperty.GetPropertyName(),
                    HandlerDataPropertyName = handlerDataProperty.GetPropertyName()
                }
            );
        }

        public static KeyValuePair<TypeContract, Func<TSource, JsonContent, TSource>> Maps<TContract>(Func<TContract, TSource, TSource> mapper)
        {
            return new KeyValuePair<TypeContract, Func<TSource, JsonContent, TSource>>
            (
                typeof(TContract).Contract(),
                (d, json) =>
                {
                    var contract = JsonConvert.DeserializeObject<TContract>(json.Value);
                    return mapper(contract, d);
                }
            );
        }

        public static KeyValuePair<TypeContract, Func<TSource, JsonContent, TSource>> Maps<TContract>(Func<TContract, Action<TSource>> mapper)
        {
            return new KeyValuePair<TypeContract, Func<TSource, JsonContent, TSource>>
            (
                typeof(TContract).Contract(),
                (d, json) =>
                {
                    var contract = JsonConvert.DeserializeObject<TContract>(json.Value);
                    var source = d;
                    mapper(contract)(source);
                    return source;
                }
            );
        }

        public static KeyValuePair<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>> Correlation(params Expression<Func<TSource, object>>[] properties)
        {
            return new KeyValuePair<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>>
            (
                typeof(TSource).Contract(),
                e => properties.Select(p => Property(p, (TSource)e))
            );
        }

        public static Correlation Property(Expression<Func<TSource, object>> property, TSource source)
        {
            return new Correlation
            {
                Contract = new TypeContract(source),
                PropertyName = property.GetPropertyName(),
                PropertyValue = new Lazy<string>(() => (property.Compile()(source)).ToString())
            };
        }
    }
}