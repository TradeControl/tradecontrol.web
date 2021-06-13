using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Periods
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public IList<App_vwYear> App_Years { get; set; }

        public string ActivePeriod { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            App_Years = await NodeContext.App_Years.OrderBy(t => t.YearNumber).ToListAsync();
            ActivePeriod = await NodeContext.App_ActivePeriods.Select(p => $"{p.Description}-{p.MonthName}").FirstOrDefaultAsync();
            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostPeriodEnd()
        {
            try
            {
                FinancialPeriods period = new(NodeContext);

                if (await period.ClosePeriod())
                    return RedirectToPage("./Index");
                else
                    return RedirectToPage("/Admin/EventLog/Index");
            }
            catch (Exception e)
            {
                {
                    NodeContext.ErrorLog(e);
                    throw;
                }
            }
        }

        public async Task<IActionResult> OnPostRebuildPeriods()
        {
            try
            {
                FinancialPeriods period = new(NodeContext);

                if (await period.Generate())
                    return RedirectToPage("./Index");
                else
                    return RedirectToPage("/Admin/EventLog/Index");
            }
            catch (Exception e)
            {
                {
                    NodeContext.ErrorLog(e);
                    throw;
                }
            }
        }

        public async Task<IActionResult> OnPostRebuildSystem()
        {
            try
            {
                FinancialPeriods period = new(NodeContext);

                if (await period.Rebuild())
                    return RedirectToPage("./Index");
                else
                    return RedirectToPage("/Admin/EventLog/Index");
            }
            catch (Exception e)
            {
                {
                    NodeContext.ErrorLog(e);
                    throw;
                }
            }
        }
    }
}
