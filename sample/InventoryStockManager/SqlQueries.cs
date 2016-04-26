using System.Collections.Generic;
using System.Data;
using EventSourcing;
using static Dapper.SqlMapper;
using System.Linq;
using AdoNet;

namespace InventoryStockManager
{
    static class SqlQueries
    {
        public static NotificationsByCorrelations NotificationsByCorrelations(AdoNetTransaction<ApplicationStore> transaction)
        {
            return correlations => transaction.Value.Connection
                .Query<dynamic>(
                    sql: NotificationsByCorrelationsSql,
                    transaction: transaction.Value,
                    param: correlations.AsTvp())
                .Select(x => new SerializedNotification
                {
                    Contract = new TypeContract { Value = x.EventName },
                    JsonContent = new JsonContent { Value = x.Content }
                });                
        }

        static readonly string NotificationsByCorrelationsSql = @"
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
