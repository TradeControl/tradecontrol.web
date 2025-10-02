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

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {        
        public DeleteModel(NodeContext context) : base(context) { }

        public Cash_Category Cash_Category { get; set; }

        public async Task<IActionResult> OnGetAsync(string categoryCode)
        {
            try
            { 
                if (categoryCode == null)
                    return NotFound();

                var category = from c in NodeContext.Cash_tbCategories
                               join p in NodeContext.Cash_tbPolaritys on c.CashPolarityCode equals p.CashPolarityCode
                               join t in NodeContext.Cash_tbTypes on c.CashTypeCode equals t.CashTypeCode
                               join ct in NodeContext.Cash_tbCategoryTypes on c.CategoryTypeCode equals ct.CategoryTypeCode
                               where c.CategoryCode == categoryCode
                               select new Cash_Category
                               {
                                   CategoryCode = c.CategoryCode,
                                   Category = c.Category,
                                   CategoryType = ct.CategoryType,
                                   DisplayOrder = c.DisplayOrder,
                                   CashTypeCode = c.CashTypeCode,
                                   CashPolarity = p.CashPolarity,
                                   CashType = t.CashType,
                                   IsEnabled = c.IsEnabled == 0 ? false : true
                               };

                Cash_Category = await category.FirstOrDefaultAsync();

                if (Cash_Category == null)
                    return NotFound();
                else
                {
                    await SetViewData();
                    return Page();
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string categoryCode)
        {
            try
            {
                if (categoryCode == null)
                    return NotFound();

                var tbCategory = await NodeContext.Cash_tbCategories.FindAsync(categoryCode);
                NodeContext.Cash_tbCategories.Remove(tbCategory);
                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("cashTypeCode", tbCategory.CashTypeCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}