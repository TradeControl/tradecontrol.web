using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public class Tasks
    {
        readonly NodeContext _context;

        public string TaskCode { get; private set; } = string.Empty;

        public Tasks(NodeContext context)
        {
            _context = context;
        }

        public Tasks(NodeContext context, string taskCode)
        {
            _context = context;
            TaskCode = taskCode;
        }

        #region properties
        public async Task<string> ProjectTaskCode() => await _context.ProjectTaskCode(TaskCode);
        public async Task<string> ParentTaskCode() => await _context.ParentTaskCode(TaskCode);
        public async Task<bool> IsProject() => await _context.IsTaskProject(TaskCode);
        public async Task<bool> IsFullyInvoiced() => await _context.IsTaskFullyInvoiced(TaskCode);

        public Task<bool> Exists => Task.Run(() =>
        {
            try
            {
                return _context.Task_tbTasks.Where(t => t.TaskCode == TaskCode).Any();
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        });

        public async Task<short> NextAttributeOrder() => await _context.GetTaskAtttributeOrder(TaskCode);

        public async Task<short> NextOperationNumber() => await _context.GetTaskOperationNumber(TaskCode);

        public async Task<bool> NextTaskCode(string activityCode)
        {
            TaskCode = await _context.GetNextTaskCode(activityCode);
            return TaskCode != string.Empty;
        }

        public async Task<decimal> Cost() => await _context.GetTaskCost(TaskCode);

        public async Task<string> DefaultTaxCode(string accountCode, string cashCode) => await _context.TaskTaxCodeDefault(accountCode, cashCode);

        public async Task<NodeEnum.InvoiceType> DefaultInvoiceType() => await _context.TaskInvoiceTypeDefault(TaskCode);

        public async Task<NodeEnum.DocType> DefaultDocType() => await _context.TaskDocTypeDefault(TaskCode);

        public async Task<DateTime> DefaultPaymentOn(string accountCode, DateTime actionOn) => await _context.TaskPaymentOnDefault(accountCode, actionOn);

        public async Task<string> EmailAddress() => await _context.TaskEmailAddress(TaskCode);
        #endregion

        #region methods
        public async Task<bool> AssignTaskToParent(string parentTaskCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Task.proc_AssignToParent @p0, @p1", parameters: new[] { TaskCode, parentTaskCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> ReconcileCharge()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Task.proc_ReconcileCharge @p0", parameters: new[] { TaskCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> Configure()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Task.proc_Configure @p0", parameters: new[] { await ProjectTaskCode() });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> Reschedule()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Task.proc_Schedule @p0", parameters: new[] { TaskCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<string> Copy()
        {
            string newTaskCode = await _context.TaskCopy(TaskCode);
            if (newTaskCode.Length > 0)
                TaskCode = newTaskCode;
            return newTaskCode;
        }

        public async Task<bool> Delete()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Task.proc_Delete @p0", parameters: new[] { TaskCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        /// <summary>
        /// Invoice and optionally pay a task
        /// </summary>
        /// <param name="postPayment"></param>
        /// <returns>Payment Code</returns>
        public async Task<string> Pay(bool postPayment) => await _context.TaskPay(TaskCode, postPayment);

        public async Task<bool> AddToCostSet()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Task.proc_CostSetAdd @p0", parameters: new[] { TaskCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }
        #endregion

    }
}