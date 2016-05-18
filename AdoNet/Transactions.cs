using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using EventSourcing;

namespace AdoNet
{
    public class AdoNetTransactionScope : Unit<TransactionScope>, IDisposable, EndpointConnection
    {
        public AdoNetTransactionScope()
        {
            Value = new TransactionScope();
        }

        public void Dispose()
        {
            Value.Dispose();
        }

        public static CommitWork<AdoNetTransactionScope> Commit()
        {
            return work =>
            {
                using (var scope = new AdoNetTransactionScope())
                {
                    work(scope);
                    scope.Value.Complete();
                }
            };
        }

        public TransactionScope Value { get; }
    }

    public class AdoNetTransaction<TConnectionStringName> : Unit<IDbTransaction>, IDisposable, EndpointConnection
        where TConnectionStringName : class
    {
        public AdoNetTransaction(Func<string, string> getConnectionString)
        {
            Value = Transaction(getConnectionString(typeof(TConnectionStringName).FriendlyName()));
        }
        public IDbTransaction Value { get; }

        static IDbTransaction Transaction(string connectionString)
        {
            var c = new SqlConnection(connectionString);
            c.Open();
            return c.BeginTransaction();
        }

        public void Dispose()
        {
            Value.Connection?.Dispose();

            Value.Dispose();
        }

        public static CommitWork<AdoNetTransaction<TConnectionStringName>> CommitWork(Func<string, string> getConnectionString)
        {
            return doWork =>
            {
                using (var t = new AdoNetTransaction<TConnectionStringName>(getConnectionString))
                {
                    doWork(t);
                    t.Value.Commit();
                }
            };
        }
    }

    public class AdoNetConnection<TConnectionStringName> : Unit<IDbConnection>, IDisposable, EndpointConnection
        where TConnectionStringName : class
    {
        public AdoNetConnection(Func<string, string> getConnectionString)
        {
            Value = Connection(getConnectionString(typeof(TConnectionStringName).FriendlyName()));
        }
        public IDbConnection Value { get; }

        static IDbConnection Connection(string connectionString)
        {
            var c = new SqlConnection(connectionString);
            c.Open();
            return c;
        }

        public void Dispose()
        {
            Value?.Dispose();
        }

        public static CommitWork<AdoNetConnection<TConnectionStringName>> CommitWork(Func<string, string> getConnectionString)
        {
            return doWork =>
            {
                using (var t = new AdoNetConnection<TConnectionStringName>(getConnectionString))
                {
                    doWork(t);                    
                }
            };
        }
    }
}