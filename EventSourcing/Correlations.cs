using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventSourcing
{
    public struct CorrelationMap
    {
        public static CorrelationMap For<THandlerData, TNotification>(
            Expression<Func<THandlerData, dynamic>> handlerDataProperty,
            Expression<Func<TNotification, dynamic>> notificationProperty)
        {
            return new CorrelationMap
            {
                HandlerDataContract = TypeIdentifier.For<THandlerData>(),
                NotificationContract = TypeIdentifier.For<TNotification>(),
                NotificationPropertyName = notificationProperty.GetPropertyName(),
                HandlerDataPropertyName = handlerDataProperty.GetPropertyName()
            };
        }

        public TypeIdentifier HandlerDataContract { get; set; }
        public string HandlerDataPropertyName { get; set; }
        public TypeIdentifier NotificationContract { get; set; }
        public string NotificationPropertyName { get; set; }
    }

    public struct Correlation       
    {
        public TypeIdentifier Contract { get; set; }
        public string PropertyName { get; set; }
        public Lazy<string> PropertyValue { get; set; }
    }
    
    public static class BuildCorrelationsFor
    {
        public static IEnumerable<Correlation> CorrelatedNotificationsBy(
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps,
            IEnumerable<Correlation> handlerDataCorrelations)
        {
            return handlerDataCorrelationMaps.Select(map => new Correlation
            {
                PropertyName = map.NotificationPropertyName,
                Contract = map.NotificationContract,
                PropertyValue = handlerDataCorrelations.Single(x => x.PropertyName == map.HandlerDataPropertyName).PropertyValue
            });
        }

        public static IEnumerable<Correlation> HandlerDataBy<TNotification>(
            IEnumerable<CorrelationMap> handlerDataCorrelationMaps, 
            TNotification notification)
        {
            return handlerDataCorrelationMaps.Select(m => new Correlation
            {
                PropertyName = m.HandlerDataPropertyName,
                Contract = m.HandlerDataContract,
                PropertyValue = new Lazy<string>(() => (m.NotificationPropertyName.GetPropertySelector<TNotification>().Compile()(notification)).ToString())
            });
        }        
    }
}
