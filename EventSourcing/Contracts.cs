namespace EventSourcing
{
    public interface IDomainEvent
    {}

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

    public struct TypeIdentifier : Unit<string>
    {
        public TypeIdentifier(object t)
        {
            Value = t.GetType().FriendlyName();
        }
        public static TypeIdentifier For<TContract>()
        {
            return new TypeIdentifier { Value = typeof(TContract).FriendlyName() };
        }
        public string Value { get; set; }
    }
}
