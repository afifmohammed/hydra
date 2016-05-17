using System;
using System.Collections.Generic;
using System.Configuration;
using EventSourcing;
using Hangfire;
using Hangfire.Common;
using Hangfire.SqlServer;
using Hangfire.States;

namespace AdoNet
{
    public static class Hangfire<TConnectionStringName>
    {
        static Hangfire()
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(
                nameOrConnectionString: ConnectionString(typeof(TConnectionStringName).FriendlyName()),
                options: new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(1)
                });
        }

        public static void Enqueue<TEventStoreName>(AdoNetTransactionScope endpoint, IEnumerable<SubscriberMessage> messages) where TEventStoreName : class
        {
            foreach (var subscriberMessage in messages)
            {
                var message = new JsonMessage(subscriberMessage);
                BackgroundJob.Enqueue(() => Handle<TEventStoreName>(message));
            }
        }

        [UseQueueFromParameter(0)]
        public static void Handle<TStoreName>(JsonMessage message) where TStoreName : class
        {
            JsonEventStoreMessageHandler<AdoNetTransaction<TStoreName>>.Handle(message);
        }

        public static Func<string, string> ConnectionString = name => ConfigurationManager.ConnectionStrings[name].ConnectionString;
    }

    public class UseQueueFromParameterAttribute : JobFilterAttribute, IElectStateFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueAttribute"/> class
        /// using the specified queue name.
        /// </summary>
        public UseQueueFromParameterAttribute(int parameterIndex)
        {
            this.ParameterIndex = parameterIndex;
        }

        public int ParameterIndex { get; private set; }

        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;

            if (enqueuedState == null) return;

            var parameter = context.BackgroundJob.Job.Args[ParameterIndex] as JsonMessage;

            if (ReferenceEquals(parameter, null)) return;
            enqueuedState.Queue = parameter.HandlerAddress;
        }
    }
}