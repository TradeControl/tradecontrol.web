using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        [BindProperty]
        public App_tbYear App_tbYear { get; set; }

        [BindProperty]
        public string MonthName { get; set; }
        public SelectList MonthNames { get; set; }

        [BindProperty]
        public string CashStatus { get; set; }
        public SelectList CashStatuses { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task OnGetAsync()
        {
            try
            {
                Profile profile = new(NodeContext);
                short startMonth = 1, yearNumber = (short)DateTime.Now.Year;
                string yearDesc = string.Empty;

                if (await NodeContext.App_tbYears.AnyAsync())
                {
                    startMonth = await NodeContext.App_tbYears.MaxAsync(y => y.StartMonth);
                    yearNumber = (short)(await NodeContext.App_tbYears.MaxAsync(y => y.YearNumber) + 1);
                    if (startMonth == 1)
                        yearDesc = $"{yearNumber}";
                    else
                        yearDesc = string.Concat($"{yearNumber}-", $"{yearNumber + 1}".Substring(2));
                }

                App_tbYear = new App_tbYear() {
                    YearNumber = yearNumber,
                    Description = yearDesc,
                    StartMonth = startMonth,
                    CashStatusCode = (short)NodeEnum.CashStatus.Forecast,
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User)),
                    InsertedOn = DateTime.Now
                };

                CashStatuses = new SelectList(await NodeContext.Cash_tbStatuses.OrderBy(s => s.CashStatusCode).Select(s => s.CashStatus).ToListAsync());
                MonthNames = new SelectList(await NodeContext.App_tbMonths.OrderBy(m => m.MonthNumber).Select(m => m.MonthName).ToListAsync());

                CashStatus = await NodeContext.Cash_tbStatuses
                                .Where(s => s.CashStatusCode == App_tbYear.CashStatusCode)
                                .Select(s => s.CashStatus)
                                .FirstAsync();

                MonthName = await NodeContext.App_tbMonths
                                .Where(m => m.MonthNumber == App_tbYear.StartMonth)
                                .Select(m => m.MonthName)
                                .FirstAsync();

                await SetViewData();
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

                NodeContext.App_tbYears.Add(App_tbYear);
                await NodeContext.SaveChangesAsync();

                FinancialPeriods periods = new(NodeContext);

                if (await periods.Generate())
                {
                    return RedirectToPage("./Index",
                        routeValues: new {
                            embedded = embedded ? "1" : null,
                            returnNode
                        });
                }

                return RedirectToPage("/Admin/EventLog/Index",
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
