using System;
using System.Linq.Expressions;

namespace EventSourcing
{
    public struct CorrelationMap
    {
        public static CorrelationMap For<THandlerData, TNotification>(
            Expression<Func<THandlerData, object>> handlerDataProperty,
            Expression<Func<TNotification, object>> notificationProperty)
        {
            return new CorrelationMap
            {
                HandlerDataContract = TypeContract.For<THandlerData>(),
                NotificationContract = TypeContract.For<TNotification>(),
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

        public static Correlation For<TContract>(Expression<Func<TContract, object>> property, TContract contract)
        {
            return new Correlation
            {
                Contract = new TypeContract(contract),
                PropertyName = property.GetPropertyName(),
                PropertyValue = new Lazy<string>(() => (property.Compile()(contract)).ToString())
            };
        }
    }  
}
