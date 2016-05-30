using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Hydra.Core;

namespace Hydra.AdoNet
{
    public class AdoNetTransactionScopeUowProvider : Wrapper<TransactionScope>, IDisposable, IUowProvider
    {
        public AdoNetTransactionScopeUowProvider()
        {
            Value = new TransactionScope();
        }

        public void Dispose()
        {
            Value.Dispose();
        }

        public static CommitWork<AdoNetTransactionScopeUowProvider> Commit()
        {
            return work =>
            {
                using (var scope = new AdoNetTransactionScopeUowProvider())
                {
                    work(scope);
                    scope.Value.Complete();
                }
            };
        }

        public TransactionScope Value { get; }
    }

    public class AdoNetTransactionUowProvider<TConnectionStringName> : Wrapper<IDbTransaction>, IDisposable, IUowProvider
        where TConnectionStringName : class
    {
        public AdoNetTransactionUowProvider(Func<string, string> getConnectionString)
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

        public static CommitWork<AdoNetTransactionUowProvider<TConnectionStringName>> CommitWork(Func<string, string> getConnectionString)
        {
            return doWork =>
            {
                using (var t = new AdoNetTransactionUowProvider<TConnectionStringName>(getConnectionString))
                {
                    doWork(t);
                    t.Value.Commit();
                }
            };
        }
    }

    public class AdoNetConnectionUowProvider<TConnectionStringName> : Wrapper<IDbConnection>, IDisposable, IUowProvider
        where TConnectionStringName : class
    {
        public AdoNetConnectionUowProvider(Func<string, string> getConnectionString)
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

        public static CommitWork<AdoNetConnectionUowProvider<TConnectionStringName>> CommitWork(Func<string, string> getConnectionString)
        {
            return doWork =>
            {
                using (var t = new AdoNetConnectionUowProvider<TConnectionStringName>(getConnectionString))
                {
                    doWork(t);                    
                }
            };
        }
    }
}