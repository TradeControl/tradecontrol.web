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
using TradeControl.Web.Tenancy;

namespace TradeControl.Web.AppServices.Execution
{
    public class ExecutionWorker : BackgroundService
    {
        readonly IServiceScopeFactory ScopeFactory;
        readonly ILogger<ExecutionWorker> Logger;
        readonly ITenantDirectory TenantDirectory;
        readonly HashSet<Guid> UnavailableTenants = new();

        public ExecutionWorker(IServiceScopeFactory scopeFactory, ILogger<ExecutionWorker> logger,
            ITenantDirectory tenantDirectory)
        {
            ScopeFactory = scopeFactory;
            Logger = logger;
            TenantDirectory = tenantDirectory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var tenant in TenantDirectory.ListEnabled())
            {
                try
                {
                    await RecoverInterruptedExecutionsAsync(tenant, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception e)
                {
                    LogTenantUnavailable(tenant, "recovery", e);
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var executed = false;
                    foreach (var tenant in TenantDirectory.ListEnabled())
                    {
                        try
                        {
                            var executionProcessed = await ProcessNextExecutionAsync(tenant, stoppingToken);
                            LogTenantRecovered(tenant);

                            if (!executionProcessed)
                                continue;
                            executed = true;
                            break;
                        }
                        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                        {
                            return;
                        }
                        catch (Exception e)
                        {
                            LogTenantUnavailable(tenant, "polling", e);
                        }
                    }

                    if (!executed)
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception e)
                {
                    Logger.LogError("Execution worker failure ({FailureType}).",
                        e.GetBaseException().GetType().Name);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        void LogTenantUnavailable(TenantDescriptor tenant, string operation, Exception exception)
        {
            if (!UnavailableTenants.Add(tenant.TenantId))
                return;

            Logger.LogError(
                "Execution {Operation} failed for tenant {Tenant} ({FailureType}); further failures are suppressed until recovery.",
                operation, tenant.TenantId, exception.GetBaseException().GetType().Name);
        }

        void LogTenantRecovered(TenantDescriptor tenant)
        {
            if (!UnavailableTenants.Remove(tenant.TenantId))
                return;

            Logger.LogInformation("Execution polling recovered for tenant {Tenant}.", tenant.TenantId);
        }

        async Task RecoverInterruptedExecutionsAsync(TenantDescriptor tenant, CancellationToken cancellationToken)
        {
            using var scope = ScopeFactory.CreateScope();
            scope.ServiceProvider.GetRequiredService<ITenantContext>().Bind(tenant);
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

            Logger.LogWarning("Recovered {ExecutionCount} interrupted execution(s) for tenant {Tenant}.",
                runningExecutions.Count, tenant.TenantId);
        }

        async Task<bool> ProcessNextExecutionAsync(TenantDescriptor tenant, CancellationToken cancellationToken)
        {
            using var scope = ScopeFactory.CreateScope();
            scope.ServiceProvider.GetRequiredService<ITenantContext>().Bind(tenant);
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var executionRuntimeState = scope.ServiceProvider.GetRequiredService<IExecutionRuntimeState>();
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

            executionRuntimeState.SetRunning(
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

                executionRuntimeState.SetSucceeded(
                    execution.ExecutionCode,
                    execution.ProgressMessage,
                    execution.CompletedOn.Value);
            }
            catch (Exception e)
            {
                execution.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Failed;
                execution.CompletedOn = DateTime.Now;
                var failureType = e.GetBaseException().GetType().Name;
                execution.ErrorMessage = $"Execution failed ({failureType}).";
                execution.ProgressMessage = "Execution failed.";

                executionRuntimeState.SetFailed(
                    execution.ExecutionCode,
                    execution.ProgressMessage,
                    execution.ErrorMessage,
                    execution.CompletedOn.Value);

                Logger.LogError(
                    "Execution {ExecutionCode} failed for tenant {Tenant} ({FailureType}).",
                    execution.ExecutionCode, tenant.TenantId, failureType);
            }

            await nodeContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
