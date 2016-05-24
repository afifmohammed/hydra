using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Hydra.Core;
using Newtonsoft.Json;

namespace Hydra.AdoNet
{
    public static class SqlEventStore
    {
        public static Action<NotificationsByPublisherAndVersion> SaveNotificationsByPublisherAndVersion(IDbTransaction transaction)
        {
            return notificationsByPublisherAndVersion =>
            {
                foreach (var item in notificationsByPublisherAndVersion
                    .NotificationsByPublisher
                    .Notifications
                    .Where(n => n.Item2.Any())                    
                    .Select(n => new
                    {
                        EventName = n.Item2.GroupBy(x => x.Contract).Single().Key.Value,
                        Content = JsonConvert.SerializeObject(n.Item1),
                        EventCorrelations = n.Item2,
                        When = notificationsByPublisherAndVersion.NotificationsByPublisher.When
                    }))
                {
                    transaction.Connection.ExecuteScalar(
                        sql: "AddPublisherEvents",
                        param: new
                        {
                            EventName = item.EventName,
                            Content = item.Content,
                            When = item.When,
                            EventCorrelations = item.EventCorrelations.AsTvp()
                        },
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure);
                }

                var publisher = notificationsByPublisherAndVersion
                    .NotificationsByPublisher
                    .PublisherDataCorrelations
                    .AsPublisherNameAndCorrelation();

                var rowCount = transaction.Connection.Execute(
                    sql: "UpsertPublisher",
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction,
                    param: new
                    {
                        Name = publisher.Item1,
                        Correlation = publisher.Item2,
                        Version = notificationsByPublisherAndVersion.Version.Value,
                        ExpectedVersion = notificationsByPublisherAndVersion.ExpectedVersion.Value
                    });

                if (rowCount == 0)
                    throw new DBConcurrencyException($@"Not match found for Publisher 
                        with Data contract '{publisher.Item1}' 
                        Version '{notificationsByPublisherAndVersion.ExpectedVersion}' 
                        and Correlation '{publisher.Item2}'");
            };

        }

        public static Func<IEnumerable<Correlation>, int> PublisherVersionByContractAndCorrelations(IDbTransaction transaction)
        {
            return correlations =>
            {
                return transaction.Connection
                    .Query<int>(
                        sql: "GetPublisherVersion",
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure,
                        param: correlations.AsPublisherNameAndCorrelation().As(x => new { Name = x.Item1, Correlation = x.Item2 }))
                    .FirstOrDefault();
            };
        }

        public static NotificationsByCorrelations NotificationsByCorrelations(IDbTransaction transaction)
        {
            return correlations => transaction.Connection
                .Query<dynamic>(
                    sql: "GetEventsWithCorrelations",
                    transaction: transaction,
                    param:  new { tvpEvents = correlations.AsTvp() },
                    commandType:CommandType.StoredProcedure)
                .Select(x => new SerializedNotification
                {
                    Contract = new TypeContract {Value = x.EventName},
                    JsonContent = new JsonContent {Value = x.Content}
                });
        }

        public static SqlMapper.ICustomQueryParameter AsTvp(this IEnumerable<Correlation> correlations)
        {
            var eventsDataTable = CreateEventTable();
            foreach (var item in correlations)
            {
                var contract = item.Contract.Value;
                var name = item.PropertyName;
                var value = item.PropertyValue.Value;

                eventsDataTable.Rows.Add(contract, name, value);
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

        static Tuple<string, string> AsPublisherNameAndCorrelation(this IEnumerable<Correlation> correlations)
        {
            var correlatedItems = correlations.ToList();

            return new Tuple<string, string>(
                correlatedItems.GroupBy(c => c.Contract.Value).Single().Key,
                JsonConvert.SerializeObject(correlatedItems
                    .Select(x => new {x.PropertyName, x.PropertyValue})
                    .OrderBy(x => x.PropertyName)
                    .ToDictionary(x => x.PropertyName, x => x.PropertyValue))
                );
        }

        static object As<T>(this T instance, Func<T, object> map)
        {
            return map(instance);
        }
    }
}