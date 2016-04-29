using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace InventoryStockManager
{
    class ProgramAdo
    {
        static void Main(string[] args)
        {
            var connection = new SqlConnection(@"Data Source=LFW7902179\TESTING08;database=EVENTSTORE;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

            var eventsDataTable = CreateEventTable();

            eventsDataTable.Rows.Add("updated", "Name", "Alex");
            eventsDataTable.Rows.Add("updated", "Address", "145 Smith St");
            eventsDataTable.Rows.Add("added", "Address", "145 Smith St");

            connection.ExecuteScalar(
                sql: "proc_AddPublisherEvents",
                param: new
                {
                    EventName = "Account Created",
                    Content = "{\"name\":\"Tom\"}",
                    When = DateTimeOffset.Now,
                    EventCorrelations = eventsDataTable.AsTableValuedParameter("dbo.EventTableType")
                },
                commandType: CommandType.StoredProcedure);

            var results = connection.Query<EventContent>("SELECT ec.EventName, e.Content FROM [Events] as e INNER JOIN EventCorrelations AS ec ON e.EventId = ec.EventId INNER JOIN @tvpEvents as t ON e.EventName = t.EventName AND ec.PropertyName = t.PropertyName AND ec.PropertyValue = t.PropertyValue ORDER BY e.[When] DESC",
                new { tvpEvents = eventsDataTable.AsTableValuedParameter("dbo.EventTableType") }).ToList();


            foreach (var result in results)
            {
                Console.WriteLine("{0}\t{1}", result.EventName, result.Content);
            }

            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
        }

        public class EventContent
        {
            public string EventName { get; set; }

            public string Content { get; set; }
        }

        private static DataTable CreateEventTable()
        {
            var eventTable = new DataTable("Event");

            // Define one column.
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