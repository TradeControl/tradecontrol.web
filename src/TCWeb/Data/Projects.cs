using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public class Projects
    {
        readonly NodeContext _context;

        public string ProjectCode { get; private set; } = string.Empty;

        public Projects(NodeContext context)
        {
            _context = context;
        }

        public Projects(NodeContext context, string projectCode)
        {
            _context = context;
            ProjectCode = projectCode;
        }

        #region properties
        public async Task<string> ProjectProjectCode() => await _context.ProjectHeaderCode(ProjectCode);
        public async Task<string> ParentProjectCode() => await _context.ParentProjectCode(ProjectCode);
        public async Task<bool> IsProject() => await _context.IsProject(ProjectCode);
        public async Task<bool> IsFullyInvoiced() => await _context.IsProjectFullyInvoiced(ProjectCode);

        public async Task<bool> Exists()
        {
            try
            {
                return await _context.Project_tbProjects.Where(t => t.ProjectCode == ProjectCode).AnyAsync();
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<short> NextAttributeOrder() => await _context.GetTaskAtttributeOrder(ProjectCode);

        public async Task<short> NextOperationNumber() => await _context.GetTaskOperationNumber(ProjectCode);

        public async Task<bool> NextProjectCode(string activityCode)
        {
            ProjectCode = await _context.GetNextProjectCode(activityCode);
            return ProjectCode != string.Empty;
        }

        public async Task<decimal> Cost() => await _context.GetTaskCost(ProjectCode);

        public async Task<string> DefaultTaxCode(string accountCode, string cashCode) => await _context.TaskTaxCodeDefault(accountCode, cashCode);

        public async Task<NodeEnum.InvoiceType> DefaultInvoiceType() => await _context.TaskInvoiceTypeDefault(ProjectCode);

        public async Task<NodeEnum.DocType> DefaultDocType() => await _context.TaskDocTypeDefault(ProjectCode);

        public async Task<DateTime> DefaultPaymentOn(string accountCode, DateTime actionOn) => await _context.ProjectPaymentOnDefault(accountCode, actionOn);

        public async Task<string> EmailAddress() => await _context.TaskEmailAddress(ProjectCode);
        #endregion

        #region methods
        public async Task<bool> AssignTaskToParent(string parentProjectCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Project.proc_AssignToParent @p0, @p1", parameters: new[] { ProjectCode, parentProjectCode });

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> ReconcileCharge()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Project.proc_ReconcileCharge @p0", parameters: new[] { ProjectCode });

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> Configure()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Project.proc_Configure @p0", parameters: new[] { await ProjectProjectCode() });

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> Reschedule()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Project.proc_Schedule @p0", parameters: new[] { ProjectCode });

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<string> Copy()
        {
            string newProjectCode = await _context.ProjectCopy(ProjectCode);
            if (newProjectCode.Length > 0)
                ProjectCode = newProjectCode;
            return newProjectCode;
        }

        public async Task<bool> Delete()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Project.proc_Delete @p0", parameters: new[] { ProjectCode });

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        /// <summary>
        /// Invoice and optionally pay a project
        /// </summary>
        /// <param name="postPayment"></param>
        /// <returns>Payment Code</returns>
        public async Task<string> Pay(bool postPayment) => await _context.ProjectPay(ProjectCode, postPayment);

        public async Task<bool> AddToCostSet()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Project.proc_CostSetAdd @p0", parameters: new[] { ProjectCode });

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }
        #endregion

    }
}