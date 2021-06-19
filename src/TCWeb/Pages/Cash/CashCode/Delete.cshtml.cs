using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;


namespace TradeControl.Web.Pages.Cash.CashCode
{
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

        public Cash_vwCode Cash_Code { get; set; }

        public async Task<IActionResult> OnGetAsync(string cashCode)
        {
            if (cashCode == null)
                return NotFound();

            Cash_Code = await NodeContext.Cash_Codes.Where(c => c.CashCode == cashCode).FirstOrDefaultAsync();

            if (Cash_Code == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string cashCode)
        {
            try
            {
                if (cashCode == null)
                    return NotFound();

                var tbCashCode = await NodeContext.Cash_tbCodes.FindAsync(cashCode);
                NodeContext.Cash_tbCodes.Remove(tbCashCode);
                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("Category", await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == tbCashCode.CategoryCode).Select(c => c.Category).FirstAsync());

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
