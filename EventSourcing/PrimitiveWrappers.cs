using System;

namespace EventSourcing
{
    public interface Unit<TValue>
    {
        TValue Value { get; }
    }

    public struct JsonContent : Unit<string>
    {
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

        public static TypeContract For<TContract>()
        {
            return new TypeContract { Value = typeof(TContract).FriendlyName() };
        }

        public string Value { get; private set; }
    }
}
