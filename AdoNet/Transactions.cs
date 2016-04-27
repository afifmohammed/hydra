using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using EventSourcing;

namespace AdoNet
{
    public class AdoNetTransactionScope : Unit<TransactionScope>, IDisposable
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

    public class AdoNetTransaction<TStore> : Unit<IDbTransaction>, IDisposable
        where TStore : class
    {
        public AdoNetTransaction(Func<string, string> getConnectionString)
        {
            Value = Transaction(getConnectionString(typeof(TStore).FriendlyName()));
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

        public static CommitWork<AdoNetTransaction<TStore>> CommitWork(Func<string, string> getConnectionString)
        {
            return doWork =>
            {
                using (var t = new AdoNetTransaction<TStore>(getConnectionString))
                {
                    doWork(t);
                    t.Value.Commit();
                }
            };
        }
    }
}