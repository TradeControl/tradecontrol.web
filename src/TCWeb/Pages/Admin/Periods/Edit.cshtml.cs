using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Periods
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context) : base(context) { }

        [BindProperty]
        public App_tbYear App_tbYear { get; set; }

        [BindProperty]
        public string CashStatus { get; set; }
        public SelectList CashStatuses { get; set; }

        [BindProperty]
        public string MonthName { get; set; }
        public SelectList MonthNames { get; set; }

        public IList<App_vwYearPeriod> App_YearPeriods { get; set; }

        public async Task<IActionResult> OnGetAsync(short? yearNumber)
        {
            try
            {
                if (yearNumber == null)
                    return NotFound();

                App_tbYear = await NodeContext.App_tbYears.Where(y => y.YearNumber == yearNumber).FirstOrDefaultAsync();

                if (App_tbYear == null)
                    return NotFound();

                CashStatuses = new SelectList(await NodeContext.Cash_tbStatuses.OrderBy(s => s.CashStatusCode).Select(s => s.CashStatus).ToListAsync());

                CashStatus = await NodeContext.Cash_tbStatuses
                                        .Where(s => s.CashStatusCode == App_tbYear.CashStatusCode)
                                        .Select(s => s.CashStatus)
                                        .FirstAsync();

                MonthNames = new SelectList(await NodeContext.App_tbMonths.OrderBy(m => m.MonthNumber).Select(m => m.MonthName).ToListAsync());

                MonthName = await NodeContext.App_tbMonths
                                        .Where(m => m.MonthNumber == App_tbYear.StartMonth)
                                        .Select(m => m.MonthName)
                                        .FirstAsync();

                App_YearPeriods = await NodeContext.App_YearPeriods
                                    .Where(p => p.YearNumber == yearNumber)
                                    .OrderBy(p => p.StartOn)
                                    .ToListAsync();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? "Periods")
                    : "Periods";

                App_tbYear.CashStatusCode = await NodeContext.Cash_tbStatuses
                    .Where(s => s.CashStatus == CashStatus)
                    .Select(s => s.CashStatusCode)
                    .FirstAsync();

                App_tbYear.StartMonth = await NodeContext.App_tbMonths
                                .Where(m => m.MonthName == MonthName)
                                .Select(m => m.MonthNumber)
                                .FirstAsync();

                NodeContext.Attach(App_tbYear).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.App_tbYears.AnyAsync(e => e.YearNumber == App_tbYear.YearNumber))
                        return base.NotFound();
                    else
                        throw;
                }

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
