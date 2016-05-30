using Hydra.AdoNet;
using Hydra.Core;
using Hydra.RequestPipeline;
using Hydra.Requests;
using Hydra.Subscriptions;

namespace WebApi
{
    /// <summary>
    /// This class is used as place holder to specify that the name of the connection string is <see cref="EventStoreTransportConnectionString"/>
    /// </summary>
    class EventStoreTransportConnectionString { }

    class ApplicationSubscriptionDispatcher
    {
        public static Response<Unit> Dispatch<TRequest>(TRequest command) where TRequest : IRequest<Unit>, ICorrelated
        {
            return SubscriptionDispatcher<AdoNetTransactionScopeProvider>.Dispatch(command);
        }
    }
}
