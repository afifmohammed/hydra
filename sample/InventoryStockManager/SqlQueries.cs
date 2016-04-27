using System;
using System.Collections.Generic;
using System.Data;
using EventSourcing;
using static Dapper.SqlMapper;
using System.Linq;
using Newtonsoft.Json;

namespace InventoryStockManager
{
    static class SqlQueries
    {
        public static Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersion(IDbTransaction transaction)
        {
            return x =>
            {
                foreach (var tuple in x.NotificationsByPublisher.Notifications)
                {
                    IDomainEvent notification = tuple.Item1; // todo: save the notification, get the id back
                    IEnumerable<Correlation> correlations = tuple.Item2; // todo: save the correlations using the id from above
                }

                var publisher = x.NotificationsByPublisher.PublisherDataCorrelations.AsPublisherNameAndCorrelation();

                var rowCount = transaction.Connection.ExecuteScalar<int>(
                    sql: x.ExpectedVersion.Value == 0 
                        ? @"INSERT INTO Publishers (Name, Correlation, Version)
                            VALUES (@Name, @Correlation, @Version)" 
                        : @"UPDATE Publisher SET Version = @Version 
                            WHERE Name = @Name 
                            AND Correlation = @Correlation 
                            AND Version = @ExpectedVersion",
                    transaction: transaction,
                    param: new { Name = publisher.Item1, Correlation = publisher.Item2, x.Version, x.ExpectedVersion });

                if (rowCount == 0)
                    throw new DBConcurrencyException($@"
                    Not match found for Publisher with Data contract '{publisher.Item1}' Version '{x.ExpectedVersion}' and Correlation '{publisher.Item2}'");
            }; 

        }
        static object As<T>(this T instance, Func<T, object> map)
        {
            return map(instance);
        }

        public static Func<IEnumerable<Correlation>, int> PublisherVersionByContractAndCorrelations(IDbTransaction transaction)
        {
            return correlations => transaction.Connection
                .Query<int>(
                    sql: @"
                        SELECT Version
                        FROM Publishers
                        WHERE Name = @Name
                            AND Correlation = @Correlation;",
                    transaction: transaction,
                    param: correlations.AsPublisherNameAndCorrelation().As(x => new {Name = x.Item1, Correlation = x.Item2}))
                .FirstOrDefault();
        }

        static Tuple<string, string> AsPublisherNameAndCorrelation(this IEnumerable<Correlation> correlations)
        {
            return new Tuple<string, string>(
                correlations.GroupBy(c => c.Contract.Value).Single().Key,
                JsonConvert.SerializeObject(correlations
                    .Select(x => new { x.PropertyName, x.PropertyValue })
                    .OrderBy(x => x.PropertyName)
                    .ToDictionary(x => x.PropertyName, x => x.PropertyValue))
            );
        } 

        public static NotificationsByCorrelations NotificationsByCorrelations(IDbTransaction transaction)
        {
            return correlations => transaction.Connection
                .Query<dynamic>(
                    sql: NotificationsByCorrelationsSql,
                    transaction: transaction,
                    param: correlations.AsTvp())
                .Select(x => new SerializedNotification
                {
                    Contract = new TypeContract { Value = x.EventName },
                    JsonContent = new JsonContent { Value = x.Content }
                });                
        }

        private const string NotificationsByCorrelationsSql = @"
			SELECT e.EventName, e.Content
			FROM [Events] as e 
			INNER JOIN EventCorrelations AS ec 
				ON e.EventId = ec.EventId 
			INNER JOIN @tvpEvents as t 
				ON e.EventName = t.EventName 
				AND ec.PropertyName = t.PropertyName 
				AND ec.PropertyValue = t.PropertyValue 
			Group by e.EventId, e.EventName, e.Content
			ORDER BY e.EventId";

        public static ICustomQueryParameter AsTvp(this IEnumerable<Correlation> correlations)
        {
            var eventsDataTable = CreateEventTable();
            foreach (var item in correlations)
            {
                eventsDataTable.Rows.Add(item.Contract, item.PropertyName, item.PropertyValue.Value);
            }

            return eventsDataTable.AsTableValuedParameter("dbo.EventTableType");
        }

        static DataTable CreateEventTable()
        {
            var eventTable = new DataTable("Event");
            
            var colEventName = new DataColumn("EventName", typeof(string));
            eventTable.Columns.Add(colEventName);
            var colPropertyName = new DataColumn("PropertyName", typeof(string));
            eventTable.Columns.Add(colPropertyName);
            var colPropertyValue = new DataColumn("PropertyValue", typeof(string));
            eventTable.Columns.Add(colPropertyValue);
            return eventTable;
        }
    }
}
