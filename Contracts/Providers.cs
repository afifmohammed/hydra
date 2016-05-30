namespace Hydra.Core
{
    public interface IUowProvider { }
    
    public delegate void DoWork<in TUowProvider>(TUowProvider provider) where TUowProvider : IUowProvider;

    public delegate void CommitWork<out TUowProvider>(DoWork<TUowProvider> work) where TUowProvider : IUowProvider;
}