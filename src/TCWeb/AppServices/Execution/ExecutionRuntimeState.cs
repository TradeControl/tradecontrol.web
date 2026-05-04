using System;
using System.Collections.Generic;
using System.Linq;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.Execution
{
    public interface IExecutionRuntimeState
    {
        void SetQueued(string executionCode, NodeEnum.ExecutionType executionType, DateTime queuedOn, string progressMessage);
        void SetRunning(string executionCode, NodeEnum.ExecutionType executionType, DateTime queuedOn, DateTime startedOn, string progressMessage);
        void UpdateProgress(string executionCode, string progressMessage);
        void SetSucceeded(string executionCode, string progressMessage, DateTime completedOn);
        void SetFailed(string executionCode, string progressMessage, string errorMessage, DateTime completedOn);
        bool TryGet(string executionCode, out ExecutionRuntimeSnapshot execution);
        bool TryGetActiveDatabaseMaintenance(out ExecutionRuntimeSnapshot execution);
    }

    public sealed class ExecutionRuntimeState : IExecutionRuntimeState
    {
        readonly object SyncRoot = new();
        readonly Dictionary<string, ExecutionRuntimeSnapshot> Executions = new(StringComparer.OrdinalIgnoreCase);

        public void SetQueued(string executionCode, NodeEnum.ExecutionType executionType, DateTime queuedOn, string progressMessage)
        {
            lock (SyncRoot)
            {
                CleanupExpiredEntries();

                Executions[executionCode] = new ExecutionRuntimeSnapshot {
                    ExecutionCode = executionCode,
                    ExecutionType = executionType.ToString(),
                    ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Pending,
                    ExecutionStatus = GetStatusText(NodeEnum.ExecutionStatus.Pending),
                    QueuedOn = queuedOn,
                    ProgressMessage = progressMessage,
                    LastUpdatedOn = DateTime.Now
                };
            }
        }

        public void SetRunning(string executionCode, NodeEnum.ExecutionType executionType, DateTime queuedOn, DateTime startedOn, string progressMessage)
        {
            lock (SyncRoot)
            {
                CleanupExpiredEntries();

                var current = GetOrCreate(executionCode, executionType, queuedOn);
                current.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Running;
                current.ExecutionStatus = GetStatusText(NodeEnum.ExecutionStatus.Running);
                current.StartedOn = startedOn;
                current.CompletedOn = null;
                current.ProgressMessage = progressMessage;
                current.ErrorMessage = null;
                current.LastUpdatedOn = DateTime.Now;
            }
        }

        public void UpdateProgress(string executionCode, string progressMessage)
        {
            lock (SyncRoot)
            {
                if (!Executions.TryGetValue(executionCode, out var current))
                    return;

                current.ProgressMessage = progressMessage;
                current.LastUpdatedOn = DateTime.Now;
            }
        }

        public void SetSucceeded(string executionCode, string progressMessage, DateTime completedOn)
        {
            lock (SyncRoot)
            {
                if (!Executions.TryGetValue(executionCode, out var current))
                    return;

                current.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Succeeded;
                current.ExecutionStatus = GetStatusText(NodeEnum.ExecutionStatus.Succeeded);
                current.CompletedOn = completedOn;
                current.ProgressMessage = progressMessage;
                current.ErrorMessage = null;
                current.LastUpdatedOn = DateTime.Now;
            }
        }

        public void SetFailed(string executionCode, string progressMessage, string errorMessage, DateTime completedOn)
        {
            lock (SyncRoot)
            {
                if (!Executions.TryGetValue(executionCode, out var current))
                    return;

                current.ExecutionStatusCode = (short)NodeEnum.ExecutionStatus.Failed;
                current.ExecutionStatus = GetStatusText(NodeEnum.ExecutionStatus.Failed);
                current.CompletedOn = completedOn;
                current.ProgressMessage = progressMessage;
                current.ErrorMessage = errorMessage;
                current.LastUpdatedOn = DateTime.Now;
            }
        }

        public bool TryGet(string executionCode, out ExecutionRuntimeSnapshot execution)
        {
            lock (SyncRoot)
            {
                CleanupExpiredEntries();

                if (Executions.TryGetValue(executionCode, out var current))
                {
                    execution = current.Clone();
                    return true;
                }

                execution = null;
                return false;
            }
        }

        public bool TryGetActiveDatabaseMaintenance(out ExecutionRuntimeSnapshot execution)
        {
            lock (SyncRoot)
            {
                CleanupExpiredEntries();

                execution = Executions.Values
                    .Where(e => e.ExecutionType == NodeEnum.ExecutionType.SyntheticDataset.ToString()
                        && e.ExecutionStatusCode == (short)NodeEnum.ExecutionStatus.Running)
                    .OrderByDescending(e => e.StartedOn ?? e.QueuedOn)
                    .Select(e => e.Clone())
                    .FirstOrDefault();

                return execution != null;
            }
        }

        ExecutionRuntimeSnapshot GetOrCreate(string executionCode, NodeEnum.ExecutionType executionType, DateTime queuedOn)
        {
            if (!Executions.TryGetValue(executionCode, out var current))
            {
                current = new ExecutionRuntimeSnapshot {
                    ExecutionCode = executionCode,
                    ExecutionType = executionType.ToString(),
                    QueuedOn = queuedOn
                };

                Executions[executionCode] = current;
            }

            current.ExecutionType = executionType.ToString();
            current.QueuedOn = queuedOn;
            return current;
        }

        void CleanupExpiredEntries()
        {
            var expiry = DateTime.Now.AddMinutes(-30);

            var expiredKeys = Executions
                .Where(item => item.Value.CompletedOn.HasValue && item.Value.LastUpdatedOn < expiry)
                .Select(item => item.Key)
                .ToList();

            foreach (var key in expiredKeys)
                Executions.Remove(key);
        }

        static string GetStatusText(NodeEnum.ExecutionStatus status) => status.ToString();
    }

    public sealed class ExecutionRuntimeSnapshot
    {
        public string ExecutionCode { get; set; }
        public string ExecutionType { get; set; }
        public short ExecutionStatusCode { get; set; }
        public string ExecutionStatus { get; set; }
        public DateTime QueuedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string ProgressMessage { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public ExecutionRuntimeSnapshot Clone()
        {
            return new ExecutionRuntimeSnapshot {
                ExecutionCode = ExecutionCode,
                ExecutionType = ExecutionType,
                ExecutionStatusCode = ExecutionStatusCode,
                ExecutionStatus = ExecutionStatus,
                QueuedOn = QueuedOn,
                StartedOn = StartedOn,
                CompletedOn = CompletedOn,
                ProgressMessage = ProgressMessage,
                ErrorMessage = ErrorMessage,
                LastUpdatedOn = LastUpdatedOn
            };
        }
    }
}
