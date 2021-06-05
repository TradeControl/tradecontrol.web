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
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class EditModel : DI_BasePageModel
    {
        public SelectList CashDescriptions { get; set; }
        [BindProperty]
        public string CashDescription { get; set; }

        public SelectList TaxDescriptions { get; set; }
        [BindProperty]
        public string TaxDescription { get; set; }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

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

        public EditModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        public async Task<IActionResult> OnGetAsync(string paymentCode, string cashCode, string taxCode)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentCode) && string.IsNullOrEmpty(PaymentCode))
                    return NotFound();
                else if (!string.IsNullOrEmpty(paymentCode))
                {
                    PaymentCode = paymentCode;
                    CashCode = string.Empty;
                    TaxCode = string.Empty;
                }

                Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.FirstOrDefaultAsync(m => m.PaymentCode == PaymentCode);

                if (Cash_PaymentsUnposted == null)
                    return NotFound();
                else
                {
                    if ((User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole)) == false)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        if (Cash_PaymentsUnposted.UserId != await profile.UserId(user.Id))
                            return Forbid();
                    }

                }

                if (Cash_PaymentsUnposted.CashCode != null)
                {
                    var cashDescriptions = from t in NodeContext.Cash_CodeLookup
                                           where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                           orderby t.CashDescription
                                           select t.CashDescription;

                    CashDescriptions = new SelectList(await cashDescriptions.ToListAsync());

                    if (!string.IsNullOrEmpty(cashCode))
                        CashCode = cashCode;
                    else if (string.IsNullOrEmpty(CashCode))
                        CashCode = Cash_PaymentsUnposted.CashCode;

                    CashDescription = await NodeContext.Cash_tbCodes.Where(c => c.CashCode == CashCode).Select(c => c.CashDescription).FirstOrDefaultAsync();

                    var taxDescriptions = from t in NodeContext.App_TaxCodes
                                          orderby t.TaxDescription
                                          select t.TaxDescription;

                    TaxDescriptions = new SelectList(await taxDescriptions.ToListAsync());

                    if (!string.IsNullOrEmpty(taxCode))
                        TaxCode = taxCode;
                    else if (string.IsNullOrEmpty(TaxCode) && !string.IsNullOrEmpty(CashCode))
                    {
                        CashCodes cash = new(NodeContext, CashCode);
                        TaxCode = cash.TaxCode;
                    }
                    else
                        TaxCode = Cash_PaymentsUnposted.TaxCode;

                    TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();
                }

                await SetViewData();

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

                Profile profile = new(NodeContext);

                if (!string.IsNullOrEmpty(Cash_PaymentsUnposted.CashCode))
                {
                    Cash_PaymentsUnposted.CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstAsync();
                    Cash_PaymentsUnposted.TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();
                }

                Cash_PaymentsUnposted.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

                NodeContext.Attach(Cash_PaymentsUnposted).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Cash_PaymentsUnposted.AnyAsync(e => e.PaymentCode == Cash_PaymentsUnposted.PaymentCode))
                        return NotFound();
                    else
                    {
                        NodeContext.ErrorLog(new DbUpdateConcurrencyException());
                        throw;
                    }
                }

                RouteValueDictionary route = new();
                route.Add("CashAccountCode", Cash_PaymentsUnposted.CashAccountCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)

            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }


        public async Task<IActionResult> OnPostGetCashCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Index?returnUrl=/Cash/PaymentEntry/Edit");
        }

        public async Task<IActionResult> OnPostNewCashCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Create?returnUrl=/Cash/PaymentEntry/Edit");
        }

        public async Task<IActionResult> OnPostGetTaxCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Admin/TaxCode/Index?returnUrl=/Cash/PaymentEntry/Edit");
        }

        public async Task<IActionResult> OnPostNewTaxCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Admin/TaxCode/Create?returnUrl=/Cash/PaymentEntry/Edit");
        }

        async Task SaveSession()
        {
            try
            {
                CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstAsync();
                TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();
            }
            catch
            {

            }
        }
    }
}
