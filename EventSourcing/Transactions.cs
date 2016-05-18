namespace EventSourcing
{
    public interface EndpointConnection { }
    
    public delegate void DoWork<in TEndpointConnection>(TEndpointConnection endpoint) where TEndpointConnection : EndpointConnection;

    public delegate void CommitWork<out TEndpointConnection>(DoWork<TEndpointConnection> work) where TEndpointConnection : EndpointConnection;
}