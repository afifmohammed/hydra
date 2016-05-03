using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EventSourcing
{
    public static class Extensions
    {
        public static IEnumerable<Correlation> Correlations(this IDomainEvent notification)
        {
            return notification.Correlations.Select(c => new Correlation
            {
                Contract = new TypeContract(notification),
                PropertyName = c.Key,
                PropertyValue = new Lazy<string>(() => c.Value.ToString())
            });
        } 

        public static KeyValuePair<string, object> PropertyNameValue<T>(this T instance, Expression<Func<T, object>> property)
        {
            return new KeyValuePair<string, object>(property.GetPropertyName(), property.Compile()(instance));
        }

        public static TypeContract Contract(this Type t)
        {
            return new TypeContract(t);
        }

        public static TypeContract Contract(this object t)
        {
            return new TypeContract(t);
        }

        public static TValue Get<TKey, TValue>(this IDictionary<string, object> dictionary)
        {
            return (TValue)dictionary[typeof(TKey).FriendlyName()];
        }

        public static object GetPropertyValue<T>(this string propertyName, T instance)
        {
            var properties = propertyName.Split('.');
            var result = properties.First().GetPropertySelector<T>().Compile()(instance);
            if (ReferenceEquals(result, null)) return null;

            return !properties.Skip(1).Any()
                ? result
                : typeof(Extensions).GetMethod("GetPropertyValue").MakeGenericMethod(result.GetType())
                    .Invoke(null, new[] { properties.Skip(1).Aggregate((x, xs) => string.Join(".", x, xs)), result });
        }

        public static string GetPropertyName<T>(this Expression<Func<T, object>> property)
        {
            LambdaExpression lambda = property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression)(lambda.Body);
                memberExpression = (MemberExpression)(unaryExpression.Operand);
            }
            else
            {
                memberExpression = (MemberExpression)(lambda.Body);
            }
            var names = new List<string> {((PropertyInfo) memberExpression.Member).Name};

            var nested = memberExpression.Expression as MemberExpression;

            while (nested != null)
            {
                names.Add(((PropertyInfo)nested.Member).Name);
                nested = nested.Expression as MemberExpression;
            }

            names.Reverse();
            return names.Aggregate((x, xs) => x + "." + xs);
        }

        static Expression<Func<T, object>> GetPropertySelector<T>(this string propertyName)
        {
            var arg = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(arg, propertyName);
            var conv = Expression.Convert(property, typeof(object));
            var exp = Expression.Lambda<Func<T, object>>(conv, arg);
            return exp;
        }

        public static string FriendlyName(this Type type)
        {
            if (type.GetGenericArguments().Length == 0)
            {
                return type.Name;
            }
            var genericArguments = type.GetGenericArguments();
            var typeDefeninition = type.Name;
            var unmangledName = typeDefeninition.Substring(0, typeDefeninition.IndexOf("`"));
            return unmangledName + "(of " + string.Join(",", genericArguments.Select(FriendlyName)) + ")";
        }

        public static T With<T>(this T instance, Action<T> operation)
        {
            operation(instance);
            return instance;
        }
    }
}
