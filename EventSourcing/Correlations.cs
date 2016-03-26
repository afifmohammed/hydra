using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace EventSourcing
{
    public struct CorrelationMap
    {
        public static CorrelationMap Between<THandlerData, TNotification>(
            Expression<Func<THandlerData, object>> handlerDataProperty,
            Expression<Func<TNotification, object>> notificationProperty)
        {
            return new CorrelationMap
            {
                HandlerDataContract = typeof(THandlerData).Contract(),
                NotificationContract = typeof(TNotification).Contract(),
                NotificationPropertyName = notificationProperty.GetPropertyName(),
                HandlerDataPropertyName = handlerDataProperty.GetPropertyName()
            };
        }

        public TypeContract HandlerDataContract { get; set; }
        public string HandlerDataPropertyName { get; set; }
        public TypeContract NotificationContract { get; set; }
        public string NotificationPropertyName { get; set; }
    }

    public static partial class Type<TContract> where TContract : new()
    {
        public static KeyValuePair<TypeContract, Func<TContract, JsonContent, TContract>> Maps<TSource>(Func<TSource, Action<TContract>> mapper)
        {
            return new KeyValuePair<TypeContract, Func<TContract, JsonContent, TContract>>
            (
                typeof(TSource).Contract(),
                (d, json) =>
                {
                    var notification = JsonConvert.DeserializeObject<TSource>(json.Value);
                    var contract = d;
                    mapper(notification)(contract);
                    return contract;                    
                }
            );
        }

        public static KeyValuePair<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>> Correlation(params Expression<Func<TContract, object>>[] properties)
        {
            return new KeyValuePair<TypeContract, Func<IDomainEvent, IEnumerable<Correlation>>>(typeof(TContract).Contract(), e => properties.Select(p => Property(p, (TContract)e)));
        }

        public static Correlation Property(Expression<Func<TContract, object>> property, TContract contract)
        {
            return new Correlation
            {
                Contract = new TypeContract(contract),
                PropertyName = property.GetPropertyName(),
                PropertyValue = new Lazy<string>(() => (property.Compile()(contract)).ToString())
            };
        }
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
}
