using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.AppServices.Execution
{
    public class ExecutionQueue : IExecutionQueue
    {
        readonly NodeContext NodeContext;
        readonly IExecutionRuntimeState ExecutionRuntimeState;

        public ExecutionQueue(NodeContext nodeContext, IExecutionRuntimeState executionRuntimeState)
        {
            NodeContext = nodeContext;
            ExecutionRuntimeState = executionRuntimeState;
        }

        public async Task<string> EnqueueAsync(NodeEnum.ExecutionType executionType, string arguments, string queuedBy = null)
        {
            string executionCode;

            do
            {
                executionCode = string.Concat("EX", Guid.NewGuid().ToString("N").Substring(0, 18));
            }
            while (await NodeContext.App_tbExecutions.AnyAsync(e => e.ExecutionCode == executionCode));

            var execution = new App_tbExecution {
                ExecutionCode = executionCode,
                ExecutionType = executionType.ToString(),
                ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Pending,
                QueuedBy = string.IsNullOrWhiteSpace(queuedBy) ? null : queuedBy,
                QueuedOn = DateTime.Now,
                Arguments = arguments,
                ProgressMessage = "Queued"
            };

            NodeContext.App_tbExecutions.Add(execution);
            await NodeContext.SaveChangesAsync();

            ExecutionRuntimeState.SetQueued(
                execution.ExecutionCode,
                executionType,
                execution.QueuedOn,
                execution.ProgressMessage);

            return executionCode;
        }
    }
}
