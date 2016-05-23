namespace EventSourcing
{
    public interface IProvider { }
    
    public delegate void DoWork<in TProvider>(TProvider provider) where TProvider : IProvider;

    public delegate void CommitWork<out TProvider>(DoWork<TProvider> work) where TProvider : IProvider;
}