using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.Execution
{
    public class ExecutionWorker : BackgroundService
    {
        readonly IServiceScopeFactory ScopeFactory;
        readonly ILogger<ExecutionWorker> Logger;
        readonly IExecutionRuntimeState ExecutionRuntimeState;

        public ExecutionWorker(IServiceScopeFactory scopeFactory, ILogger<ExecutionWorker> logger, IExecutionRuntimeState executionRuntimeState)
        {
            ScopeFactory = scopeFactory;
            Logger = logger;
            ExecutionRuntimeState = executionRuntimeState;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RecoverInterruptedExecutionsAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var executed = await ProcessNextExecutionAsync(stoppingToken);

                    if (!executed)
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Execution worker failure.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        async Task RecoverInterruptedExecutionsAsync(CancellationToken cancellationToken)
        {
            using var scope = ScopeFactory.CreateScope();

            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var runningExecutions = await nodeContext.App_tbExecutions
                .Where(e => e.ExecutionStatusCode == (short)NodeEnum.ExecutionStatus.Running)
                .ToListAsync(cancellationToken);

            if (!runningExecutions.Any())
                return;

            var completedOn = DateTime.Now;

            foreach (var execution in runningExecutions)
            {
                execution.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Failed;
                execution.CompletedOn = completedOn;
                execution.ErrorMessage = "Execution stopped before completion.";
                execution.ProgressMessage = "Execution failed.";
            }

            await nodeContext.SaveChangesAsync(cancellationToken);

            Logger.LogWarning("Recovered {ExecutionCount} interrupted execution(s).", runningExecutions.Count);
        }

        async Task<bool> ProcessNextExecutionAsync(CancellationToken cancellationToken)
        {
            using var scope = ScopeFactory.CreateScope();

            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var handlers = scope.ServiceProvider.GetServices<IExecutionHandler>()
                .ToDictionary(h => h.ExecutionType);

            var execution = await nodeContext.App_tbExecutions
                .OrderBy(e => e.QueuedOn)
                .ThenBy(e => e.ExecutionCode)
                .FirstOrDefaultAsync(e => e.ExecutionStatusCode == (short)NodeEnum.ExecutionStatus.Pending, cancellationToken);

            if (execution == null)
                return false;

            var runtimeExecutionType = Enum.TryParse(execution.ExecutionType, true, out NodeEnum.ExecutionType parsedExecutionType)
                ? parsedExecutionType
                : NodeEnum.ExecutionType.SyntheticDataset;

            execution.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Running;
            execution.StartedOn = DateTime.Now;
            execution.CompletedOn = null;
            execution.ErrorMessage = null;
            execution.ProgressMessage = "Starting execution...";
            await nodeContext.SaveChangesAsync(cancellationToken);

            ExecutionRuntimeState.SetRunning(
                execution.ExecutionCode,
                runtimeExecutionType,
                execution.QueuedOn,
                execution.StartedOn.Value,
                execution.ProgressMessage);

            try
            {
                if (!Enum.TryParse(execution.ExecutionType, true, out NodeEnum.ExecutionType executionType))
                    throw new InvalidOperationException($"Unknown execution type: {execution.ExecutionType}");

                if (!handlers.TryGetValue(executionType, out var handler))
                    throw new InvalidOperationException($"No handler is registered for execution type: {execution.ExecutionType}");

                await handler.ExecuteAsync(execution, cancellationToken);

                execution.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Succeeded;
                execution.CompletedOn = DateTime.Now;

                if (string.IsNullOrWhiteSpace(execution.ProgressMessage))
                    execution.ProgressMessage = "Execution completed.";

                ExecutionRuntimeState.SetSucceeded(
                    execution.ExecutionCode,
                    execution.ProgressMessage,
                    execution.CompletedOn.Value);
            }
            catch (Exception e)
            {
                execution.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Failed;
                execution.CompletedOn = DateTime.Now;
                execution.ErrorMessage = e.GetBaseException().Message;
                execution.ProgressMessage = "Execution failed.";

                ExecutionRuntimeState.SetFailed(
                    execution.ExecutionCode,
                    execution.ProgressMessage,
                    execution.ErrorMessage,
                    execution.CompletedOn.Value);

                Logger.LogError(e, "Execution {ExecutionCode} failed.", execution.ExecutionCode);
            }

            await nodeContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
