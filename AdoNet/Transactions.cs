using System;
using System.Data;
using System.Data.SqlClient;
using EventSourcing;

namespace AdoNet
{
    public class AdoNetTransaction<TStore> : Unit<IDbTransaction>, IDisposable
        where TStore : class
    {
        public AdoNetTransaction()
        {
            Value = Transaction(typeof(TStore).FriendlyName());
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
            Value.Dispose();
            Value.Connection.Dispose();
        }

        public static CommitWork<AdoNetTransaction<TStore>> CommitWork()
        {
            return doWork =>
            {
                using (var t = new AdoNetTransaction<TStore>())
                {
                    doWork(t);
                    t.Value.Commit();
                }
            };
        }
    }
}