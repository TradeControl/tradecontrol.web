using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Periods
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

        [BindProperty]
        public App_vwYear App_Year { get; set; }

        public async Task<IActionResult> OnGetAsync(short? yearNumber)
        {
            try
            {
                if (yearNumber == null)
                    return NotFound();

                App_Year = await NodeContext.App_Years.Where(y => y.YearNumber == yearNumber).FirstOrDefaultAsync();

                if (App_Year == null)
                    return NotFound();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(short? yearNumber)
        {
            try
            {
                if (yearNumber == null)
                    return NotFound();

                var tbYear = await NodeContext.App_tbYears.FindAsync(yearNumber);

                if (tbYear == null)
                    return NotFound();

                NodeContext.App_tbYears.Remove(tbYear);
                await NodeContext.SaveChangesAsync();

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? "Periods")
                    : "Periods";

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
