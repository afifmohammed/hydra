using System;
using System.Configuration;
using AdoNet;
using EventSourcing;
using RequestPipeline;

namespace WebApi
{
    /// <summary>
    /// This class is used as place holder to specify that the name of the connection string is <see cref="ApplicationStore"/>
    /// </summary>
    class ApplicationStore
    { }

    class EventStoreTransport { }

    class ApplicationRequestPipeline
    {
        public static Response<Unit> Dispatch<TCommand>(TCommand command) where TCommand : IRequest<Unit>, ICorrelated
        {
            return RequestPipeline<AdoNetTransactionScope>.Dispatch(command);
        }  
    }
}
