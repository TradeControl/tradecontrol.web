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

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        const string SessionKeyReturnUrl = "_returnUrlCashCodeCreate";

        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        [BindProperty]
        public Cash_tbCategory Cash_tbCategory { get; set; }

        public SelectList CashModes { get; set; }
        public SelectList CashTypes { get; set; }

        [BindProperty]       
        public string CashMode { get; set; }
        [BindProperty]
        public string CashType { get; set; }

        public CreateModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var modes = NodeContext.Cash_tbModes.OrderBy(m => m.CashModeCode).Select(m => m.CashMode);
                CashModes = new SelectList(await modes.ToListAsync());
                CashMode = await modes.FirstAsync();

                var types = NodeContext.Cash_tbTypes.OrderBy(t => t.CashTypeCode).Select(t => t.CashType);
                CashTypes = new SelectList(await types.ToListAsync());
                CashType = await types.FirstAsync();

                Profile profile = new(NodeContext);
                var userName = await profile.UserName(UserManager.GetUserId(User));

                Cash_tbCategory = new Cash_tbCategory()
                {
                    CategoryTypeCode = (short)NodeEnum.CategoryType.CashCode,
                    DisplayOrder = 0,
                    IsEnabled = 1,
                    CashModeCode = (short)NodeEnum.CashMode.Expense,
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

                Cash_tbCategory.CashTypeCode = await NodeContext.Cash_tbTypes.Where(t => t.CashType == CashType).Select(t => t.CashTypeCode).FirstAsync();
                Cash_tbCategory.CashModeCode = await NodeContext.Cash_tbModes.Where(m => m.CashMode == CashMode).Select(m => m.CashModeCode).FirstAsync();

                NodeContext.Cash_tbCategories.Add(Cash_tbCategory);
                await NodeContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(ReturnUrl))
                    return LocalRedirect($"{ReturnUrl}?categorycode={Cash_tbCategory.CategoryCode}");
                else
                {
                    RouteValueDictionary route = new();
                    route.Add("cashTypeCode", Cash_tbCategory.CashTypeCode);

                    return RedirectToPage("./Index", route);
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
