namespace Hydra.Configuration
{
    /// <summary>
    /// Placed holder class off which configurations for the event store can hang as extension methods
    /// </summary>    
    public class EventStoreConfiguration
    { }

    /// <summary>
    /// Placed holder class off which configurations for the event store can hang as extension methods
    /// </summary>    
    public class EventStoreConfiguration<TConstraint> : EventStoreConfiguration
        where TConstraint : class
    { }

    /// <summary>
    /// Placed holder class off which configurations for the event store can hang as extension methods
    /// </summary>    
    public class EventStoreConfiguration<TConstraint1, TConstraint2> : EventStoreConfiguration<TConstraint1>
        where TConstraint1 : class
        where TConstraint2 : class
    { }
}
