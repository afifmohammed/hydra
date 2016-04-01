using System;
using Newtonsoft.Json;

namespace EventSourcing
{
    public interface Unit<out TValue>
    {
        TValue Value { get; }
    }

    public struct JsonContent : Unit<string>
    {
        public JsonContent(object obj)
        {
            Value = JsonConvert.SerializeObject(obj);
        }
        public JsonContent(string value)
        {
            Value = value;
        }
        public string Value { get; set; }
    }

    public struct Version : Unit<int>
    {
        public Version(int value)
        {
            Value = value;
        }
        public int Value { get; set; }
    }

    public struct TypeContract : Unit<string>
    {
        public TypeContract(Type t)
        {
            Value = t.FriendlyName();
        }

        public TypeContract(object t)
        {
            Value = t.GetType().FriendlyName();
        }

        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if ((obj is TypeContract) == false) return false;

            return Equals((TypeContract)obj);
        }

        public bool Equals(TypeContract other)
        {
            return string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
