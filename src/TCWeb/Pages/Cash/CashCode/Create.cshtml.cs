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
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
namespace TradeControl.Web.Pages.Cash.CashCode
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        #region session
        const string SessionKeyReturnUrl = "_returnUrlCashCodeCreate";
        const string SessionKeyTaxCode = "_TaxCode";
        const string SessionKeyCashCode = "_CashCode";
        const string SessionKeyCategoryCode = "_CategoryCode";
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

        string CategoryCode
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

        string TaxCode
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
        public Cash_tbCode CashCodeNew { get; set; }

        public SelectList CategoryCodes { get; set; }
        public SelectList TaxCodes { get; set; }

        public CreateModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task OnGetAsync(string returnUrl, string taxcode, string categorycode)
        {
            await SetViewData();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                TaxCode = string.Empty;
                CashCode = string.Empty;
                CategoryCode = string.Empty;
                CashDescription = string.Empty;
                ReturnUrl = returnUrl;
            }

            if (!string.IsNullOrEmpty(taxcode))
                TaxCode = taxcode;

            if (!string.IsNullOrEmpty(categorycode))
                CategoryCode = categorycode;

            TaxCodes = new SelectList(await NodeContext.App_TaxCodes.OrderBy(t => t.TaxCode).Select(t => t.TaxCode).ToListAsync());
            CategoryCodes = new SelectList(await NodeContext.Cash_tbCategories
                                                        .Where(t => t.CategoryTypeCode == (short)NodeEnum.CategoryType.CashCode)
                                                        .OrderBy(t => t.CategoryCode)
                                                        .Select(t => t.CategoryCode)
                                                        .ToListAsync());
            Profile profile = new(NodeContext);
            var userName = await profile.UserName(UserManager.GetUserId(User));

            CashCodeNew = new Cash_tbCode
            {
                CashCode = CashCode,
                CashDescription = CashDescription,
                CategoryCode = CategoryCode,
                TaxCode = TaxCode,
                IsEnabled = -1,
                InsertedBy = userName,
                UpdatedBy = userName,
                InsertedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };


        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            NodeContext.Cash_tbCodes.Add(CashCodeNew);
            await NodeContext.SaveChangesAsync();

            if (!string.IsNullOrEmpty(ReturnUrl))
                return LocalRedirect($"{ReturnUrl}?cashcode={CashCodeNew.CashCode}");
            else
                return RedirectToPage("./Index");
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
            return LocalRedirect(@"/Admin/TaxCode/Index?returnUrl=/Cash/CashCode/Create");
        }

        public IActionResult OnPostNewTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Admin/TaxCode/Create?returnUrl=/Cash/CashCode/Create");
        }

        void SaveSession()
        {
            try
            {
                TaxCode = CashCodeNew?.TaxCode;
                CashCode = CashCodeNew?.CashCode;
                CashDescription = CashCodeNew?.CashDescription;
                CategoryCode = CashCodeNew?.CategoryCode;
            }
            catch
            {

            }
        }
    }
}
