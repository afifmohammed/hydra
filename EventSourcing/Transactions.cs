namespace EventSourcing
{
    public delegate void DoWork<in TEndpoint>(TEndpoint endpoint);

    public delegate void CommitWork<out TEndpoint>(DoWork<TEndpoint> work);
}