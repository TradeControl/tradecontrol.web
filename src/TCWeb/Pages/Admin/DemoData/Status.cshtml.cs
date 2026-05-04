using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.AppServices.Execution;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.DemoData
{
    [Authorize(Roles = "Administrators")]
    public class StatusModel : DI_BasePageModel
    {
        readonly IExecutionRuntimeState ExecutionRuntimeState;

        public ExecutionStatusInfo Execution { get; set; }

        public StatusModel(NodeContext context, IExecutionRuntimeState executionRuntimeState) : base(context)
        {
            ExecutionRuntimeState = executionRuntimeState;
        }

        async Task<ExecutionStatusInfo> LoadExecutionAsync(string executionCode)
        {
            if (ExecutionRuntimeState.TryGet(executionCode, out var runtimeExecution))
                return ToExecutionStatusInfo(runtimeExecution);

            return await (from e in NodeContext.App_tbExecutions.AsNoTracking()
                          join s in NodeContext.App_tbExecutionStatuses.AsNoTracking()
                              on e.ExecutionStatusCode equals s.ExecutionStatusCode
                          where e.ExecutionCode == executionCode
                          select new ExecutionStatusInfo {
                              ExecutionCode = e.ExecutionCode,
                              ExecutionType = e.ExecutionType,
                              ExecutionStatusCode = e.ExecutionStatusCode,
                              ExecutionStatus = s.ExecutionStatus,
                              QueuedOn = e.QueuedOn,
                              StartedOn = e.StartedOn,
                              CompletedOn = e.CompletedOn,
                              ProgressMessage = e.ProgressMessage,
                              ErrorMessage = e.ErrorMessage
                          })
                         .SingleOrDefaultAsync();
        }

        static ExecutionStatusInfo ToExecutionStatusInfo(ExecutionRuntimeSnapshot execution)
        {
            return new ExecutionStatusInfo {
                ExecutionCode = execution.ExecutionCode,
                ExecutionType = execution.ExecutionType,
                ExecutionStatusCode = execution.ExecutionStatusCode,
                ExecutionStatus = execution.ExecutionStatus,
                QueuedOn = execution.QueuedOn,
                StartedOn = execution.StartedOn,
                CompletedOn = execution.CompletedOn,
                ProgressMessage = execution.ProgressMessage,
                ErrorMessage = execution.ErrorMessage
            };
        }

        public async Task<IActionResult> OnGetAsync(string executionCode)
        {
            if (string.IsNullOrWhiteSpace(executionCode))
                return RedirectToPage("./Config");

            Execution = await LoadExecutionAsync(executionCode);

            if (Execution == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnGetStateAsync(string executionCode)
        {
            if (string.IsNullOrWhiteSpace(executionCode))
                return NotFound();

            var execution = await LoadExecutionAsync(executionCode);

            if (execution == null)
                return NotFound();

            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return new JsonResult(new {
                executionCode = execution.ExecutionCode,
                executionType = execution.ExecutionType,
                executionStatusCode = execution.ExecutionStatusCode,
                executionStatus = execution.ExecutionStatus,
                progressMessage = execution.ProgressMessage,
                errorMessage = execution.ErrorMessage,
                queuedOn = execution.QueuedOn.ToString("g"),
                startedOn = execution.StartedOn?.ToString("g"),
                completedOn = execution.CompletedOn?.ToString("g"),
                isComplete = execution.IsComplete,
                isSucceeded = execution.IsSucceeded
            });
        }
    }

    public class ExecutionStatusInfo
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

        public bool IsComplete => ExecutionStatusCode == (short)NodeEnum.ExecutionStatus.Succeeded
            || ExecutionStatusCode == (short)NodeEnum.ExecutionStatus.Failed
            || ExecutionStatusCode == (short)NodeEnum.ExecutionStatus.Cancelled;

        public bool IsSucceeded => ExecutionStatusCode == (short)NodeEnum.ExecutionStatus.Succeeded;
    }
}
