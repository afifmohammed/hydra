using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Hydra.Core;

namespace Hydra.AdoNet
{
    public class AdoNetTransactionScopeProvider : Wrapper<TransactionScope>, IDisposable, IProvider
    {
        public AdoNetTransactionScopeProvider()
        {
            Value = new TransactionScope();
        }

        public void Dispose()
        {
            Value.Dispose();
        }

        public static CommitWork<AdoNetTransactionScopeProvider> Commit()
        {
            return work =>
            {
                using (var scope = new AdoNetTransactionScopeProvider())
                {
                    work(scope);
                    scope.Value.Complete();
                }
            };
        }

        public TransactionScope Value { get; }
    }

    public class AdoNetTransactionProvider<TConnectionStringName> : Wrapper<IDbTransaction>, IDisposable, IProvider
        where TConnectionStringName : class
    {
        public AdoNetTransactionProvider(Func<string, string> getConnectionString)
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

        public static CommitWork<AdoNetTransactionProvider<TConnectionStringName>> CommitWork(Func<string, string> getConnectionString)
        {
            return doWork =>
            {
                using (var t = new AdoNetTransactionProvider<TConnectionStringName>(getConnectionString))
                {
                    doWork(t);
                    t.Value.Commit();
                }
            };
        }
    }

    public class AdoNetConnectionProvider<TConnectionStringName> : Wrapper<IDbConnection>, IDisposable, IProvider
        where TConnectionStringName : class
    {
        public AdoNetConnectionProvider(Func<string, string> getConnectionString)
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

        public static CommitWork<AdoNetConnectionProvider<TConnectionStringName>> CommitWork(Func<string, string> getConnectionString)
        {
            return doWork =>
            {
                using (var t = new AdoNetConnectionProvider<TConnectionStringName>(getConnectionString))
                {
                    doWork(t);                    
                }
            };
        }
    }
}