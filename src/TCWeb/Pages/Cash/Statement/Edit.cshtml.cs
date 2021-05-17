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
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Statement
{
    public class EditModel : DI_BasePageModel
    {

        public EditModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public SelectList CashCodes { get; set; }
        public SelectList TaxCodes { get; set; }

        [BindProperty]
        public Cash_tbPayment Cash_Payment { get; set; }

        #region session data
        const string SessionKeyPaymentCode = "_paymentCode";
        const string SessionKeyCashCode = "_cashCode";
        const string SessionKeyTaxCode = "_taxCode";

        string PaymentCode
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyPaymentCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyPaymentCode, value);
            }
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

        public async Task<IActionResult> OnGetAsync(string paymentCode, string cashcode, string taxcode)
        {
            if (string.IsNullOrEmpty(paymentCode) && string.IsNullOrEmpty(PaymentCode))
                return NotFound();
            else if (!string.IsNullOrEmpty(paymentCode))
            {
                PaymentCode = paymentCode;
                CashCode = string.Empty;
                TaxCode = string.Empty;
            }

            Cash_Payment = await NodeContext.Cash_tbPayments.FirstOrDefaultAsync(m => m.PaymentCode == PaymentCode);

            if (Cash_Payment == null)
                return NotFound();
            else
            {
                if ((User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole)) == false)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    if (Cash_Payment.UserId != await profile.UserId(user.Id))
                        return Forbid();
                }
            }

            if (Cash_Payment.CashCode != null)
            {
                var cashCodes = from t in NodeContext.Cash_CodeLookup
                                where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                orderby t.CashCode
                                select t.CashCode;

                CashCodes = new SelectList(await cashCodes.Distinct().ToListAsync());

                var taxCodes = from t in NodeContext.App_TaxCodes
                               orderby t.TaxCode
                               select t.TaxCode;

                TaxCodes = new SelectList(await taxCodes.Distinct().ToListAsync());
            }

            if (!string.IsNullOrEmpty(cashcode))
                CashCode = cashcode;

            if (!string.IsNullOrEmpty(taxcode))
                TaxCode = taxcode;

            if (!string.IsNullOrEmpty(CashCode))
                Cash_Payment.CashCode = CashCode;

            if (!string.IsNullOrEmpty(TaxCode))
                Cash_Payment.TaxCode = TaxCode;

            await SetViewData();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Profile profile = new(NodeContext);
            Cash_Payment.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

            NodeContext.Attach(Cash_Payment).State = EntityState.Modified;

            try
            {
                await NodeContext.SaveChangesAsync();
                Orgs org = new(NodeContext, Cash_Payment.AccountCode);
                await org.Rebuild();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Cash_tbPaymentExists(Cash_Payment.PaymentCode))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            RouteValueDictionary route = new();
            route.Add("PaymentCode", Cash_Payment.PaymentCode);

            return RedirectToPage("./Index", route);
        }

        private bool Cash_tbPaymentExists(string paymentCode)
        {
            return NodeContext.Cash_tbPayments.Any(e => e.PaymentCode == paymentCode);
        }

        public IActionResult OnPostGetCashCode()
        {
            SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Index?returnUrl=/Cash/Statement/Edit");
        }

        public IActionResult OnPostNewCashCode()
        {
            SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Create?returnUrl=/Cash/Statement/Edit");
        }

        public IActionResult OnPostGetTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Admin/TaxCode/Index?returnUrl=/Cash/Statement/Edit");
        }

        public IActionResult OnPostNewTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Admin/TaxCode/Create?returnUrl=/Cash/Statement/Edit");
        }

        void SaveSession()
        {
            try
            {
                TaxCode = Cash_Payment?.TaxCode;
                CashCode = Cash_Payment?.CashCode;
            }
            catch
            {

            }
        }
    }
}
