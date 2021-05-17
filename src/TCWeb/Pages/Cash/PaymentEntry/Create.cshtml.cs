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

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        { }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

        public SelectList CashAccountCodes { get; set; }
        public SelectList AccountCodes { get; set; }
        public SelectList CashCodes { get; set; }
        public SelectList TaxCodes { get; set; }

        #region session data
        const string SessionKeyLoadMode = "_loadMode";
        const string SessionKeyCashAccountCode = "_CashAccountCode";
        const string SessionKeyAccountCode = "_AccountCode";
        const string SessionKeyCashCode = "_CashCode";
        const string SessionKeyTaxCode = "_TaxCode";

        public int InputMode 
        {
            get
            {                
                try
                {
                    var mode = HttpContext.Session.GetInt32(SessionKeyLoadMode);
                    return (int)mode;
                }
                catch
                {
                    InputMode = 1;
                    return 1;
                }
            }
            set
            {
                HttpContext.Session.SetInt32(SessionKeyLoadMode, value);
            }
        }

        string CashAccountCode
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyCashAccountCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyCashAccountCode, value);
            }
        }

        string AccountCode
        {
            get
            {
                try
                { 
                    return HttpContext.Session.GetString(SessionKeyAccountCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyAccountCode, value);
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

        public async Task<IActionResult> OnGetAsync(string cashAccountCode, string mode, string accountCode, string cashCode, string taxCode)
        {

            var cashAccountCodes = from t in NodeContext.Org_tbAccounts
                               where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                               orderby t.CashAccountCode
                               select t.CashAccountCode;

            CashAccountCodes = new SelectList(await cashAccountCodes.ToListAsync());

            var accountCodes = from t in NodeContext.Org_AccountLookup
                              orderby t.AccountCode
                              select t.AccountCode;

            AccountCodes = new SelectList(await accountCodes.ToListAsync());

            if (!string.IsNullOrEmpty(cashAccountCode))
                CashAccountCode = cashAccountCode;
            else if (string.IsNullOrEmpty(CashAccountCode))
            {
                CashAccounts cashAccounts = new (NodeContext);
                CashAccountCode = await cashAccounts.CurrentAccount();
            }

            if (!string.IsNullOrEmpty(mode))
            {
                InputMode = Int32.Parse(mode);
                AccountCode = string.Empty;
                CashCode = string.Empty;
                TaxCode = string.Empty;
            }

            if (!string.IsNullOrEmpty(accountCode))
                AccountCode = accountCode;
            else if (string.IsNullOrEmpty(AccountCode))
                AccountCode = accountCodes.FirstOrDefault();

            if (InputMode == 1)
            {
                var cashCodes = from t in NodeContext.Cash_CodeLookup
                                where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                orderby t.CashCode
                                select t.CashCode;

                CashCodes = new SelectList(await cashCodes.ToListAsync());

                if (!string.IsNullOrEmpty(cashCode))
                    CashCode = cashCode;
                else if (string.IsNullOrEmpty(CashCode))
                    CashCode = cashCodes.FirstOrDefault();

                var taxCodes = from t in NodeContext.App_TaxCodes
                            orderby t.TaxCode
                            select t.TaxCode;

                TaxCodes = new SelectList(await taxCodes.ToListAsync());

                if (!string.IsNullOrEmpty(taxCode))
                    TaxCode = taxCode;
                else if (string.IsNullOrEmpty(TaxCode) && !string.IsNullOrEmpty(CashCode))
                {
                    CashCodes cash = new(NodeContext, CashCode);
                    TaxCode = cash.TaxCode;
                }
            }

            Profile profile = new(NodeContext);

            Cash_PaymentsUnposted = new Cash_vwPaymentsUnposted
            {
                CashAccountCode = CashAccountCode,
                AccountCode = AccountCode,
                CashCode = CashCode,
                TaxCode = TaxCode,
                PaidOn = DateTime.Today,
                UserId = await profile.UserId(UserManager.GetUserId(User)),
                UpdatedBy = await profile.UserName(UserManager.GetUserId(User)),
                IsProfitAndLoss = false
            };

            Cash_PaymentsUnposted.InsertedBy = Cash_PaymentsUnposted.UpdatedBy;

            Orgs orgs = new (NodeContext, AccountCode);

            var balance = await orgs.BalanceOutstanding();

            if (balance < 0)
                Cash_PaymentsUnposted.PaidOutValue = Math.Abs(balance);
            else
                Cash_PaymentsUnposted.PaidInValue = balance;

            await SetViewData();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CashAccounts cashAccounts = new (NodeContext);
            Cash_PaymentsUnposted.PaymentCode = await cashAccounts.NextPaymentCode();

            Cash_PaymentsUnposted.UpdatedOn = DateTime.Now;
            Cash_PaymentsUnposted.InsertedOn = DateTime.Now;

            if (!ModelState.IsValid || (Cash_PaymentsUnposted.PaidInValue + Cash_PaymentsUnposted.PaidOutValue == 0))
                return Page();

            NodeContext.Cash_PaymentsUnposted.Add(Cash_PaymentsUnposted);
            await NodeContext.SaveChangesAsync();

            RouteValueDictionary route = new ();
            route.Add("CashAccountCode", Cash_PaymentsUnposted.CashAccountCode);

            return RedirectToPage("./Index", route);
        }

        public IActionResult OnPostNewAccountCode()
        {
            SaveSession();    
            return LocalRedirect(@"/Org/Edit/Create?returnUrl=/Cash/PaymentEntry/Create");
        }

        public IActionResult OnPostGetAccountCode()
        {
            SaveSession();
            return LocalRedirect(@"/Org/Index?returnUrl=/Cash/PaymentEntry/Create");
        }

        public IActionResult OnPostGetCashCode()
        {
            SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Index?returnUrl=/Cash/PaymentEntry/Create");
        }

        public IActionResult OnPostNewCashCode()
        {
            SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Create?returnUrl=/Cash/PaymentEntry/Create");
        }

        public IActionResult OnPostGetTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Admin/TaxCode/Index?returnUrl=/Cash/PaymentEntry/Create");
        }

        public IActionResult OnPostNewTaxCode()
        {
            SaveSession();
            return LocalRedirect(@"/Admin/TaxCode/Create?returnUrl=/Cash/PaymentEntry/Create");
        }


        void SaveSession()
        {
            try
            {
                CashAccountCode = Cash_PaymentsUnposted?.CashAccountCode;
                AccountCode = Cash_PaymentsUnposted?.AccountCode;
                TaxCode = Cash_PaymentsUnposted?.TaxCode;
                CashCode = Cash_PaymentsUnposted?.CashCode;
            }
            catch
            {

            }
        }

    }
}
