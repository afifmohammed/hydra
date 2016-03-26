using System;
using System.Linq.Expressions;

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

    public struct Correlation       
    {
        public TypeContract Contract { get; set; }
        public string PropertyName { get; set; }
        public Lazy<string> PropertyValue { get; set; }

        public static Correlation Property<TContract>(Expression<Func<TContract, object>> property, TContract contract)
        {
            return new Correlation
            {
                Contract = new TypeContract(contract),
                PropertyName = property.GetPropertyName(),
                PropertyValue = new Lazy<string>(() => (property.Compile()(contract)).ToString())
            };
        }

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
