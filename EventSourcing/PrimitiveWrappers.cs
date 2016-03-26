using System;
using Newtonsoft.Json;

namespace EventSourcing
{
    public interface Unit<TValue>
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
        public string Value { get; private set; }
    }

    public struct Version : Unit<int>
    {
        public Version(int value)
        {
            Value = value;
        }
        public int Value { get; private set; }
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

        public string Value { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeContract))
                return false;

            var other = (TypeContract)obj;

            return other.Value.Equals(Value);
        }
    }
}
