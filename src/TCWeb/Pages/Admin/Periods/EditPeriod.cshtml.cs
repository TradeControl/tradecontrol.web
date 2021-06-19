using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Periods
{
    [Authorize(Roles = "Administrators")]
    public class EditPeriodModel : DI_BasePageModel
    {
        public EditPeriodModel(NodeContext context) : base(context) { }

        public string PeriodDescription { get; set; }

        [BindProperty]
        public App_tbYearPeriod App_tbYearPeriod { get; set; }

        [BindProperty]
        public string CashStatus { get; set; }
        public SelectList CashStatuses { get; set; }

        public async Task<IActionResult> OnGetAsync(short? yearNumber, short? monthNumber)
        {
            try
            {
                if (yearNumber == null || monthNumber == null)
                    return NotFound();

                App_tbYearPeriod = await NodeContext.App_tbYearPeriods.Where(p => p.YearNumber == yearNumber && p.MonthNumber == monthNumber).FirstOrDefaultAsync();
                
                if (App_tbYearPeriod == null)
                    return NotFound();

                var App_YearPeriod = await NodeContext.App_YearPeriods.Where(p => p.YearNumber == yearNumber && p.MonthNumber == monthNumber).FirstOrDefaultAsync();
                PeriodDescription = string.Concat(App_YearPeriod.Description, "-", App_YearPeriod.MonthName);

                CashStatuses = new SelectList(await NodeContext.Cash_tbStatuses.OrderBy(s => s.CashStatusCode).Select(s => s.CashStatus).ToListAsync());
                CashStatus = await NodeContext.Cash_tbStatuses
                                            .Where(s => s.CashStatusCode == App_tbYearPeriod.CashStatusCode)
                                            .Select(s => s.CashStatus).FirstAsync();                                        

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                App_tbYearPeriod.CashStatusCode = await NodeContext.Cash_tbStatuses
                    .Where(s => s.CashStatus == CashStatus)
                    .Select(s => s.CashStatusCode)
                    .FirstAsync();

                NodeContext.Attach(App_tbYearPeriod).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.App_tbYearPeriods.AnyAsync(e => e.YearNumber == App_tbYearPeriod.YearNumber && e.MonthNumber == App_tbYearPeriod.MonthNumber))
                        return base.NotFound();
                    else
                        throw;
                }

                RouteValueDictionary route = new();
                route.Add("YearNumber", App_tbYearPeriod.YearNumber);

                return RedirectToPage("./Edit", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
