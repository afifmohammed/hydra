using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace EventSourcing
{
    public struct CorrelationMap
    {
        public TypeContract HandlerDataContract { get; set; }
        public string HandlerDataPropertyName { get; set; }
        public TypeContract NotificationContract { get; set; }
        public string NotificationPropertyName { get; set; }
    }

    public struct Correlation       
    {
        public TypeContract Contract { get; set; }
        public string PropertyName { get; set; }
        public Lazy<string> PropertyValue { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Correlation))
                return false;

            var other = (Correlation)obj;

            return other.Contract.Equals(Contract)
                && other.PropertyName == PropertyName
                && other.PropertyValue.Value == PropertyValue.Value;
        }
    }

    public static partial class Type<TSource> where TSource : new()
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
            return new KeyValuePair<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>>(typeof(TSource).Contract(), e => properties.Select(p => Property(p, (TSource)e)));
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
