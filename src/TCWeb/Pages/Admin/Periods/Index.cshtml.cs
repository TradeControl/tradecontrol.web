using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Periods
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        [BindProperty]
        public IList<App_vwYear> App_Years { get; set; }

        public string ActivePeriod { get; set; }

        [TempData]
        public string? FlashMessage { get; set; }

        [TempData]
        public bool FlashOk { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            App_Years = await NodeContext.App_Years.OrderBy(t => t.YearNumber).ToListAsync();
            ActivePeriod = await NodeContext.App_ActivePeriods.Select(p => $"{p.Description}-{p.MonthName}").FirstOrDefaultAsync();
            await SetViewData();
            return Page();
        }

        private (bool Embedded, string ReturnNode) GetEmbedStateFromForm()
        {
            var embedded = Request?.Form.ContainsKey("embedded") == true
                && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

            var returnNode = Request?.Form.ContainsKey("returnNode") == true
                ? (Request.Form["returnNode"].ToString() ?? "Periods")
                : "Periods";

            if (string.IsNullOrWhiteSpace(returnNode))
                returnNode = "Periods";

            return (embedded, returnNode);
        }

        public async Task<IActionResult> OnPostPeriodEnd()
        {
            try
            {
                var (embedded, returnNode) = GetEmbedStateFromForm();

                FinancialPeriods period = new(NodeContext);

                if (await period.ClosePeriod())
                {
                    var newActive = await NodeContext.App_ActivePeriods
                        .Select(p => $"{p.Description}-{p.MonthName}")
                        .FirstOrDefaultAsync();

                    FlashOk = true;
                    FlashMessage = string.IsNullOrWhiteSpace(newActive)
                        ? "Period End completed successfully."
                        : $"Active Period set to {newActive}.";

                    return RedirectToPage("./Index",
                        routeValues: new {
                            embedded = embedded ? "1" : null,
                            returnNode
                        });
                }

                FlashOk = false;
                FlashMessage = "Period End failed. Check Event Logs for details.";

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                FlashOk = false;
                FlashMessage = "Period End failed due to an unexpected error. Check Event Logs for details.";

                var (embedded, returnNode) = GetEmbedStateFromForm();

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
        }

        public async Task<IActionResult> OnPostRebuildPeriods()
        {
            try
            {
                var (embedded, returnNode) = GetEmbedStateFromForm();

                FinancialPeriods period = new(NodeContext);

                if (await period.Generate())
                {
                    FlashOk = true;
                    FlashMessage = "Periods rebuilt successfully.";

                    return RedirectToPage("./Index",
                        routeValues: new {
                            embedded = embedded ? "1" : null,
                            returnNode
                        });
                }

                FlashOk = false;
                FlashMessage = "Rebuild Periods failed. Check Event Logs for details.";

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                FlashOk = false;
                FlashMessage = "Rebuild Periods failed due to an unexpected error. Check Event Logs for details.";

                var (embedded, returnNode) = GetEmbedStateFromForm();

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
        }

        public async Task<IActionResult> OnPostRebuildSystem()
        {
            try
            {
                var (embedded, returnNode) = GetEmbedStateFromForm();

                FinancialPeriods period = new(NodeContext);

                if (await period.Rebuild())
                {
                    FlashOk = true;
                    FlashMessage = "System rebuilt successfully.";

                    return RedirectToPage("./Index",
                        routeValues: new {
                            embedded = embedded ? "1" : null,
                            returnNode
                        });
                }

                FlashOk = false;
                FlashMessage = "Rebuild System failed. Check Event Logs for details.";

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                FlashOk = false;
                FlashMessage = "Rebuild System failed due to an unexpected error. Check Event Logs for details.";

                var (embedded, returnNode) = GetEmbedStateFromForm();

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
        }
    }
}
