using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.States;

namespace SerializedInvocation
{
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
            enqueuedState.Queue = parameter.HandlerAddress.ToLower();
        }
    }
}
