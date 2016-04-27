using System;
using System.Configuration;

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
}
