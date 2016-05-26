using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hydra.Core
{
    public interface ICorrelated
    {
        [JsonIgnore]
        IEnumerable<KeyValuePair<string, object>> Correlations { get; }
    }

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
            if (ReferenceEquals(obj, null)) return false;
            if (!(obj is Correlation))
                return false;

            return Equals((Correlation)obj);
        }

        public bool Equals(Correlation other)
        {
            return Contract.Equals(other.Contract) && string.Equals(PropertyName, other.PropertyName) && Equals(PropertyValue.Value, other.PropertyValue.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Contract.GetHashCode();
                hashCode = (hashCode*397) ^ (PropertyName?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (PropertyValue?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
