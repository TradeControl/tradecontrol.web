using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TradeControl.Web.AppServices.Execution;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages
{
    [AllowAnonymous]
    public class IndexModel : DI_BasePageModel
    {
        readonly IConfiguration Configuration;
        readonly IExecutionRuntimeState ExecutionRuntimeState;

        [BindProperty]
        public App_vwIdentity App_Identity { get; set; }

        [BindProperty]
        [Display(Name = "Current Period")]
        public string CurrentPeriod { get; set; }

        [BindProperty]
        [Display(Name = "Web Version")]
        public string WebVersion { get; set; }

        [BindProperty]
        [Display(Name = "Node Version")]
        public string SqlNodeVersion { get; set; }

        public bool IsDatabaseMaintenanceActive { get; set; }

        public string DatabaseMaintenanceMessage { get; set; }

        public string MaintenanceExecutionCode { get; set; }

        public IndexModel(NodeContext context, IConfiguration configuration, IExecutionRuntimeState executionRuntimeState) : base(context)
        {
            Configuration = configuration;
            ExecutionRuntimeState = executionRuntimeState;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (TryShowDatabaseMaintenancePage())
                return Page();

            try
            {
                await SetViewData();

                App_Identity = await NodeContext.App_Identity.OrderBy(i => i.UserName).SingleOrDefaultAsync();

                if (App_Identity == null)
                {
                    NodeSettings nodeSettings = new NodeSettings(NodeContext);

                    if (nodeSettings.IsFirstUse)
                        await NodeContext.InitializeNode();

                    if (!nodeSettings.IsInitialised)
                        return RedirectToPage("/Admin/Manager/Index");
                    else
                        throw new Exception("Initialisation error");
                }
                else
                {
                    FinancialPeriods periods = new(NodeContext);
                    CurrentPeriod = $"{periods.ActiveYearDesc}-{periods.ActiveMonthName}";
                    SqlNodeVersion = Configuration.GetSection("Settings")["SqlNodeVersion"];
                    WebVersion = Configuration.GetSection("Settings")["WebVersion"];
                    return Page();
                }
            }
            catch (SqlException) when (TryShowDatabaseMaintenancePage())
            {
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }

        }

        bool TryShowDatabaseMaintenancePage()
        {
            if (!ExecutionRuntimeState.TryGetActiveDatabaseMaintenance(out var execution))
                return false;

            IsDatabaseMaintenanceActive = true;
            MaintenanceExecutionCode = execution.ExecutionCode;
            DatabaseMaintenanceMessage = string.IsNullOrWhiteSpace(execution.ProgressMessage)
                ? "Synthetic demo data installation is re-initialising the database."
                : execution.ProgressMessage;

            ViewData["CompanyName"] = "Trade Control";
            return true;
        }
    }
}
