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

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public Cash_Category Cash_Category { get; set; }

        public async Task<IActionResult> OnGetAsync(string categoryCode)
        {
            try
            { 
                if (categoryCode == null)
                    return NotFound();

                var category = from c in NodeContext.Cash_tbCategories
                                join p in NodeContext.Cash_tbModes on c.CashModeCode equals p.CashModeCode
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
                                    CashMode = p.CashMode,
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
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
