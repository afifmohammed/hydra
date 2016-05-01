using System;
using System.Configuration;
using AdoNet;
using EventSourcing;
using RequestPipeline;

namespace InventoryStockManager
{
    static class ConnectionString
    {
        public static Func<string, string> ByName = connectionStringName => ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
    }

    /// <summary>
    /// This class is used as place holder to specify that the name of the connection string is <see cref="ApplicationStore"/>
    /// </summary>
    public class ApplicationStore
    { }

    public static class ApplicationRequestPipeline
    {
        public static Response<Unit> Dispatch<TCommand>(TCommand command) 
            where TCommand : IRequest<Unit>, ICorrelated
        {
            return RequestPipeline<TCommand, AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Dispatch(command);
        }
    }
}
