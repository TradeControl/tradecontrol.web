using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Totals
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

        public Cash_Total Cash_Total { get; set; }
        public IList<string> Child_Categories { get; set; }
        public async Task<IActionResult> OnGetAsync(string categoryCode)
        {
            if (categoryCode == null)
                return NotFound();

            var cash_total =    from cat_total in NodeContext.Cash_CategoryTotals
                                join cash_type in NodeContext.Cash_tbTypes on cat_total.CashTypeCode equals cash_type.CashTypeCode
                                where cat_total.CategoryCode == categoryCode
                                select new Cash_Total
                                {
                                    CategoryCode = cat_total.CategoryCode,
                                    Category = cat_total.Category,
                                    CashType = cash_type.CashType,
                                    DisplayOrder = cat_total.DisplayOrder,
                                    InsertedBy = cat_total.InsertedBy,
                                    InsertedOn = cat_total.InsertedOn,
                                    UpdatedBy = cat_total.UpdatedBy,
                                    UpdatedOn = cat_total.UpdatedOn
                                };

            Cash_Total = await cash_total.FirstOrDefaultAsync();

            if (Cash_Total == null)
                return NotFound();
            else
            {
                var child_totals = from totals in NodeContext.Cash_tbCategoryTotals
                                   join cat_total in NodeContext.Cash_tbCategories on totals.ChildCode equals cat_total.CategoryCode
                                   where totals.ParentCode == Cash_Total.CategoryCode
                                   orderby cat_total.Category
                                   select cat_total.Category;

                Child_Categories = await child_totals.ToListAsync();

                await SetViewData();
                return Page();
            }
        }
    }
}
