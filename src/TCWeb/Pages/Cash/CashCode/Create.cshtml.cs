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
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CashCode
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        #region session
        const string SessionKeyReturnUrl = "_returnUrlCashCodeCreate";
        const string SessionKeyTaxCode = "_TaxDescription";
        const string SessionKeyCashCode = "_CashCode";
        const string SessionKeyCategoryCode = "_Category";
        const string SessionKeyCashDesc = "_CashDesc";


        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        string CashCode
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyCashCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyCashCode, value);
            }
        }

        string CashDescription
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyCashDesc);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyCashDesc, value);
            }
        }

        [BindProperty]
        public string Category
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyCategoryCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyCategoryCode, value);
            }
        }

        [BindProperty]
        public string TaxDescription
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyTaxCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyTaxCode, value);
            }
        }
        #endregion

        [BindProperty]
        public Cash_tbCode Cash_tbCode { get; set; }

        public SelectList Categories { get; set; }
        public SelectList TaxDescriptions { get; set; }

        public async Task OnGetAsync(string returnUrl, string categoryCode, string taxCode)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    TaxDescription = string.Empty;
                    CashCode = string.Empty;
                    Category = string.Empty;
                    CashDescription = string.Empty;
                    ReturnUrl = returnUrl;
                }

                TaxDescriptions = new SelectList(await NodeContext.App_TaxCodes.OrderBy(t => t.TaxCode).Select(t => t.TaxDescription).ToListAsync());
                Categories = new SelectList(await NodeContext.Cash_tbCategories
                                                            .Where(t => t.CategoryTypeCode == (short)NodeEnum.CategoryType.CashCode)
                                                            .OrderBy(t => t.Category)
                                                            .Select(t => t.Category)
                                                            .ToListAsync());

                Profile profile = new(NodeContext);

                if (!string.IsNullOrEmpty(taxCode))
                    TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == taxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();
                else if (string.IsNullOrEmpty(TaxDescription))
                {
                    Orgs orgs = new(NodeContext, await profile.CompanyAccountCode);
                    taxCode = await orgs.DefaultTaxCode();
                    TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == taxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();
                }                

                if (!string.IsNullOrEmpty(categoryCode))
                    Category = await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == categoryCode).Select(c => c.Category).FirstOrDefaultAsync();
                else if (string.IsNullOrEmpty(Category))
                {
                    Category = await NodeContext.Cash_tbCategories
                                        .Where(c => c.CategoryTypeCode == (short)NodeEnum.CategoryType.CashCode 
                                                && c.CashModeCode == (short)NodeEnum.CashMode.Income
                                                && c.CashTypeCode == (short)NodeEnum.CashType.Trade)
                                        .OrderBy(c => c.CategoryCode)
                                        .Select(c => c.Category)
                                        .FirstOrDefaultAsync();                    
                }
                
                var userName = await profile.UserName(UserManager.GetUserId(User));

                Cash_tbCode = new Cash_tbCode
                {
                    CashCode = CashCode,
                    CashDescription = CashDescription,
                    CategoryCode = await NodeContext.Cash_tbCategories.Where(c => c.Category == Category).Select(c => c.CategoryCode).FirstAsync(),
                    TaxCode = await NodeContext.App_tbTaxCodes.Where(t => t.TaxDescription == TaxDescription).Select(t => t.TaxCode).FirstAsync(),
                    IsEnabled = -1,
                    InsertedBy = userName,
                    UpdatedBy = userName,
                    InsertedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };
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

                Cash_tbCode.TaxCode = await NodeContext.App_tbTaxCodes.Where(t => t.TaxDescription == TaxDescription).Select(t => t.TaxCode).FirstAsync();
                Cash_tbCode.CategoryCode = await NodeContext.Cash_tbCategories.Where(c => c.Category == Category).Select(c => c.CategoryCode).FirstAsync();

                NodeContext.Cash_tbCodes.Add(Cash_tbCode);
                await NodeContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(ReturnUrl))
                    return LocalRedirect($"{ReturnUrl}?cashcode={Cash_tbCode.CashCode}");
                else
                {
                    RouteValueDictionary route = new();
                    route.Add("category", Category);

                    return RedirectToPage("./Index", route);
                }
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public IActionResult OnPostGetCategoryCode()
        {
            SaveSession();
            return LocalRedirect($"/Cash/CategoryCode/Index?returnUrl=/Cash/CashCode/Create&cashTypeCode={(short)NodeEnum.CashType.Trade}");
        }

        public IActionResult OnPostNewCategoryCode()
        {
            SaveSession();
            return LocalRedirect(@"/Cash/CategoryCode/Create?returnUrl=/Cash/CashCode/Create");
        }


        public IActionResult OnPostGetTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Tax/TaxCode/Search?returnUrl=/Cash/CashCode/Create");
        }

        public IActionResult OnPostNewTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Tax/TaxCode/Create?returnUrl=/Cash/CashCode/Create");
        }

        void SaveSession()
        {
            try
            {                
                CashCode = Cash_tbCode?.CashCode;
                CashDescription = Cash_tbCode?.CashDescription;                
            }
            catch
            {

            }
        }
    }
}
