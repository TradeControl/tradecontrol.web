using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Totals
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        protected UserManager<TradeControlWebUser> UserManager { get; }

        [BindProperty]
        public Cash_tbCategory Cash_tbCategory { get; set; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context) 
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await SetViewData();


                Profile profile = new(NodeContext);
                var userName = await profile.UserName(UserManager.GetUserId(User));

                Cash_tbCategory = new Cash_tbCategory()
                {
                    CategoryTypeCode = (short)NodeEnum.CategoryType.CashTotal,
                    DisplayOrder = 0,
                    IsEnabled = 1,
                    CashModeCode = (short)NodeEnum.CashMode.Neutral,
                    CashTypeCode = (short)NodeEnum.CashType.Trade,
                    InsertedBy = userName,
                    InsertedOn = DateTime.Now,
                    UpdatedBy = userName,
                    UpdatedOn = DateTime.Now
                };

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

                NodeContext.Cash_tbCategories.Add(Cash_tbCategory);
                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("categoryCode", Cash_tbCategory.CategoryCode);

                return RedirectToPage("./Edit", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}