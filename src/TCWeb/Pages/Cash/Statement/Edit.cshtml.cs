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
        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public SelectList CashDescriptions { get; set; }
        [BindProperty]
        public string CashDescription { get; set; }

        public SelectList TaxDescriptions { get; set; }
        [BindProperty]
        public string TaxDescription { get; set; }

        [BindProperty]
        public Cash_tbPayment Cash_Payment { get; set; }

        [BindProperty]
        public bool CashCodeIsEditable 
        {
            get { return !string.IsNullOrEmpty(CashCode); }
        }


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

        public async Task<IActionResult> OnGetAsync(string paymentCode, string cashCode, string taxCode)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentCode) && string.IsNullOrEmpty(PaymentCode))
                    return NotFound();
                else if (!string.IsNullOrEmpty(paymentCode))
                    PaymentCode = paymentCode;

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


                NodeEnum.CashAccountType cashAccountType = (NodeEnum.CashAccountType)await
                                                            (from p in NodeContext.Cash_tbPayments
                                                             join a in NodeContext.Org_tbAccounts
                                                                 on p.CashAccountCode equals a.CashAccountCode
                                                             where p.PaymentCode == paymentCode
                                                             select a.AccountTypeCode).SingleAsync();

                if (Cash_Payment.CashCode != null && cashAccountType != NodeEnum.CashAccountType.Asset)
                {
                    var cashDescriptions = from t in NodeContext.Cash_CodeLookup
                                           orderby t.CashDescription
                                           select t.CashDescription;

                    CashDescriptions = new SelectList(await cashDescriptions.ToListAsync());

                    if (!string.IsNullOrEmpty(paymentCode))
                        CashCode = Cash_Payment.CashCode;
                    else if (!string.IsNullOrEmpty(cashCode))
                        CashCode = cashCode;                       

                    CashDescription = await NodeContext.Cash_tbCodes.Where(c => c.CashCode == CashCode).Select(c => c.CashDescription).FirstOrDefaultAsync();

                    var taxDescriptions = from t in NodeContext.App_TaxCodes
                                          orderby t.TaxDescription
                                          select t.TaxDescription;

                    TaxDescriptions = new SelectList(await taxDescriptions.ToListAsync());

                    if (!string.IsNullOrEmpty(paymentCode))
                        TaxCode = Cash_Payment.TaxCode;
                    else if (!string.IsNullOrEmpty(taxCode))
                        TaxCode = taxCode;
                                            
                    TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();
                }


                await SetViewData();

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

                Profile profile = new(NodeContext);
                Cash_Payment.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

                if (CashCodeIsEditable)
                {
                    Cash_Payment.CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstAsync();
                    Cash_Payment.TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();
                }

                NodeContext.Attach(Cash_Payment).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                    Orgs org = new(NodeContext, Cash_Payment.AccountCode);
                    await org.Rebuild();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Cash_tbPayments.AnyAsync(e => e.PaymentCode == Cash_Payment.PaymentCode))
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
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
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
            return LocalRedirect(@"/Tax/TaxCode/Search?returnUrl=/Cash/Statement/Edit");
        }

        public IActionResult OnPostNewTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Tax/TaxCode/Create?returnUrl=/Cash/Statement/Edit");
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
