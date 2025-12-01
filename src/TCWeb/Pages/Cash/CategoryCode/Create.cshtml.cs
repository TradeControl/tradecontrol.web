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
        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }
        
        const string SessionKeyReturnUrl = "_returnUrlCashCodeCreate";

        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        [BindProperty]
        public Cash_tbCategory Cash_tbCategory { get; set; }
            
        [BindProperty]       
        public string CashPolarity { get; set; }
        public SelectList CashPolaritys { get; set; }

        [BindProperty]
        public string CashType { get; set; }
        public SelectList CashTypes { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var modes = NodeContext.Cash_tbPolaritys.OrderBy(m => m.CashPolarityCode).Select(m => m.CashPolarity);
                CashPolaritys = new SelectList(await modes.ToListAsync());
                CashPolarity = await modes.FirstAsync();

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
                    CashPolarityCode = (short)NodeEnum.CashPolarity.Expense,
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

                Cash_tbCategory.CashTypeCode = await NodeContext.Cash_tbTypes.Where(t => t.CashType == CashType).Select(t => t.CashTypeCode).FirstAsync();
                Cash_tbCategory.CashPolarityCode = await NodeContext.Cash_tbPolaritys.Where(m => m.CashPolarity == CashPolarity).Select(m => m.CashPolarityCode).FirstAsync();

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
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
