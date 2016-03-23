using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public struct Contract
    {
        public static Contract For<TContract>()
        {
            return new Contract { TypeName = typeof(TContract).FriendlyName() };
        }
        public string TypeName { get; set; }
    }

    public class CorrelationMap
    {
        public Contract HandlerDataContract { get; set; }
        public string HandlerDataPropertyName { get; set; }
        public Contract NotificationContract { get; set; }
        public string NotificationPropertyName { get; set; }
    }

    public class Correlation       
    {
        public Contract Contract { get; set; }
        public string PropertyName { get; set; }
        public Lazy<string> PropertyValue { get; set; }
    }
    
    public static class CorrelationBuilder
    {
        public static CorrelationMap Map<THandlerData, TNotification>(
            Expression<Func<THandlerData, dynamic>> handlerDataProperty, 
            Expression<Func<TNotification, dynamic>> notificationProperty)
        {
            return new CorrelationMap
            {
                HandlerDataContract = Contract.For<THandlerData>(),
                NotificationContract = Contract.For<TNotification>(),
                NotificationPropertyName = notificationProperty.GetPropertyName(),
                HandlerDataPropertyName = handlerDataProperty.GetPropertyName()
            };
        }

        public static IEnumerable<Correlation> CorrelationsBy(
            IEnumerable<CorrelationMap> correlationMaps, 
            IEnumerable<Correlation> handlerDataCorrelations)
        {
            return correlationMaps.Select(map => new Correlation
            {
                PropertyName = map.NotificationPropertyName,
                Contract = map.NotificationContract,
                PropertyValue = handlerDataCorrelations.Single(x => x.PropertyName == map.HandlerDataPropertyName).PropertyValue
            });
        }

        public static IEnumerable<Correlation> CorrelationsBy<TNotification>(
            IEnumerable<CorrelationMap> correlationMaps, 
            TNotification notification)
        {
            return correlationMaps.Select(m => new Correlation
            {
                PropertyName = m.HandlerDataPropertyName,
                Contract = m.HandlerDataContract,
                PropertyValue = new Lazy<string>(() => (m.NotificationPropertyName.GetPropertySelector<TNotification>().Compile()(notification)).ToString())
            });
        }        
    }
}
