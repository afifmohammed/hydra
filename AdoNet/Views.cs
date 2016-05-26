using Hydra.Core.FluentInterfaces;

namespace Hydra.AdoNet
{
    public class Denormalizer<TView, TConnectionStringName> : ConsumerBuilder<TView, AdoNetTransactionProvider<TConnectionStringName>> where TView : new() where TConnectionStringName : class
    { }
}