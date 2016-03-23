using System.Collections.Generic;

namespace EventSourcing
{
    public class Subscriber
    {
        public Contract SubscriberContract { get; set; }
        public SerializedNotification Notification { get; set; }
    }

    public class SerializedNotification
    {
        public Contract Contract { get; set; }
        public JsonContent JsonContent { get; set; }
    }

    public class Notification
    {
        public IEnumerable<Correlation> PublisherDataCorrelations { get; set; }
        public IDomainEvent Content { get; set; }
    }

    public interface IDomainEvent
    {}

    public interface IDomainCommand
    {}
    
    public interface ICommand
    { }

    public interface Unit<TValue>
    {
        TValue Value { get; }
    }

    public class JsonContent : Unit<string>
    {
        public JsonContent(string value)
        {
            Value = value;
        }
        public string Value { get; private set; }
    }
}
