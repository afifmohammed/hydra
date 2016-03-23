using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Spike
{
    public class State
    {
        public string CustomerId { get; set; }
        public string OrderId { get; set; }
    }

    public class VisitedPage
    {
        public string ProspectId { get; set; }
        public string SessionId { get; set; }
    }

    public class CustomerRegistered
    {
        public string CustomerId { get; set; }
    }

    public class AddedToCart
    {
        public string ProspectId { get; set; }
        public string CartId { get; set; }
    }


    public class MappingTests
    {
        [Fact]
        public void WorksOutofTheBox()
        {
            var list = new List<Map>()
                .Add<State, VisitedPage>(x => x.CustomerId, y => y.ProspectId)
                .Add<State, VisitedPage>(x => x.OrderId, y => y.SessionId)
                .Add<State, CustomerRegistered>(x => x.CustomerId, y => y.CustomerId)
                .Add<State, AddedToCart>(x => x.CustomerId, y => y.ProspectId)
                .Add<State, AddedToCart>(x => x.OrderId, y => y.CartId);

            list.FindContractsBy(new VisitedPage { ProspectId = "100", SessionId = "xyz" });
        }
    }

    public class Map
    {
        public string Contract { get; set; }
        public string Notification { get; set; }
        public string ContractProperty { get; set; }
        public dynamic ContractPropertyExpression { get; set; }
        public string NotificationProperty { get; set; }
        public dynamic NotificationPropertyExpression { get; set; }
        public Lazy<string> Value { get; set; }        
    }

    public static class MapExtensions
    {
        public static List<Map> Add<TContract, TNotification>(this List<Map> maps, 
            Expression<Func<TContract, object>> contractProperty, 
            Expression<Func<TNotification, object>> notificationProperty)
        {
            maps.Add(new Map
            {
                Contract = typeof(TContract).Name,
                Notification = typeof(TNotification).Name,
                ContractProperty = contractProperty.GetPropertyName(),
                ContractPropertyExpression = contractProperty,
                NotificationProperty = notificationProperty.GetPropertyName(),
                NotificationPropertyExpression = notificationProperty
            });
            return maps;
        }

        public static List<Map> FindNotificationsBy<TContract>(this List<Map> maps, TContract contract)
        {
            return maps
                .Where(x => x.Contract == typeof(TContract).Name)
                .Select(x => new Map
                {
                    Contract = x.Contract,
                    ContractProperty = x.ContractProperty,
                    ContractPropertyExpression = x.ContractPropertyExpression,
                    Notification = x.Notification,
                    NotificationProperty = x.NotificationProperty,
                    NotificationPropertyExpression = x.NotificationPropertyExpression,
                    Value = new Lazy<string>(() => (((Expression<Func<TContract, object>>)x.ContractPropertyExpression).Compile()(contract)).ToString())
                })
                .ToList();
        }

        public static List<Map> FindContractsBy<TNotification>(this List<Map> maps, TNotification notification)
        {
            return maps
                .Where(x => x.Notification == typeof(TNotification).Name)
                .Select(x => new Map
                {
                    Contract = x.Contract,
                    ContractProperty = x.ContractProperty,
                    ContractPropertyExpression = x.ContractPropertyExpression,
                    Notification = x.Notification,
                    NotificationProperty = x.NotificationProperty,
                    NotificationPropertyExpression = x.NotificationPropertyExpression,
                    Value = new Lazy<string>(() => (((Expression<Func<TNotification, object>>)x.NotificationPropertyExpression).Compile()(notification)).ToString())
                })
                .ToList();
        }

        public static string GetPropertyName<T>(this System.Linq.Expressions.Expression<Func<T, object>> property)
        {
            System.Linq.Expressions.LambdaExpression lambda = (System.Linq.Expressions.LambdaExpression)property;
            System.Linq.Expressions.MemberExpression memberExpression;

            if (lambda.Body is System.Linq.Expressions.UnaryExpression)
            {
                System.Linq.Expressions.UnaryExpression unaryExpression = (System.Linq.Expressions.UnaryExpression)(lambda.Body);
                memberExpression = (System.Linq.Expressions.MemberExpression)(unaryExpression.Operand);
            }
            else
            {
                memberExpression = (System.Linq.Expressions.MemberExpression)(lambda.Body);
            }

            return ((PropertyInfo)memberExpression.Member).Name;
        }
    }    
}
