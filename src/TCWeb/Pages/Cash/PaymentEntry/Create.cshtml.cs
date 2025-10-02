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

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class CreateModel : DI_BasePageModel
    {
        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

        public SelectList AccountNames { get; set; }
        [BindProperty]
        public string AccountName { get; set; }

        public SelectList SubjectNames { get; set; }
        [BindProperty]
        public string SubjectName { get; set; }

        public SelectList CashDescriptions { get; set; }
        [BindProperty]
        public string CashDescription { get; set; }

        public SelectList TaxDescriptions { get; set; }
        [BindProperty]
        public string TaxDescription { get; set; }

        #region session data
        const string SessionKeyLoadMode = "_loadMode";
        const string SessionKeyAccountCode = "_AccountCode";
        const string SessionKeySubjectCode = "_SubjectCode";
        const string SessionKeyCashCode = "_CashCode";
        const string SessionKeyTaxCode = "_TaxCode";
        const string SessionKeyReturnUrl = "_returnUrl";

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

        string SubjectCode
        {
            get
            {
                try
                { 
                    return HttpContext.Session.GetString(SessionKeySubjectCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeySubjectCode, value);
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

        string ReturnUrl
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyReturnUrl);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyReturnUrl, value);
            }
        }
        #endregion

        public async Task<IActionResult> OnGetAsync(string cashSubjectCode, string mode, string accountCode, string cashCode, string taxCode, string returnUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var cashSubjectNames = from t in NodeContext.Subject_tbAccounts
                                       where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                                       orderby t.AccountName
                                       select t.AccountName;

                AccountNames = new SelectList(await cashSubjectNames.ToListAsync());

                if (!string.IsNullOrEmpty(cashSubjectCode))
                    AccountCode = cashSubjectCode;
                else if (string.IsNullOrEmpty(AccountCode))
                {
                    CashAccounts cashAccounts = new(NodeContext);
                    AccountCode = await cashAccounts.CurrentAccount();
                }

                AccountName = await NodeContext.Subject_tbAccounts.Where(t => t.AccountCode == AccountCode).Select(t => t.AccountName).FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(mode))
                {
                    InputMode = Int32.Parse(mode);
                    SubjectCode = string.Empty;
                    CashCode = string.Empty;
                    TaxCode = string.Empty;
                }

                var organisationNames = from t in NodeContext.Subject_SubjectLookup
                                        orderby t.SubjectName
                                        select t.SubjectName;

                SubjectNames = new SelectList(await organisationNames.ToListAsync());

                var profile = new Profile(NodeContext);

                if (!string.IsNullOrEmpty(accountCode))
                    SubjectCode = accountCode;
                else if (string.IsNullOrEmpty(SubjectCode))
                    SubjectCode = await profile.CompanySubjectCode();

                SubjectName = await NodeContext.Subject_tbSubjects.Where(o => o.SubjectCode == SubjectCode).Select(o => o.SubjectName).FirstOrDefaultAsync();


                if (InputMode == 1)
                {
                    var cashDescriptions = from t in NodeContext.Cash_CodeLookup
                                           where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                           orderby t.CashDescription
                                           select t.CashDescription;

                    CashDescriptions = new SelectList(await cashDescriptions.ToListAsync());

                    if (!string.IsNullOrEmpty(cashCode))
                        CashCode = cashCode;
                    else if (string.IsNullOrEmpty(CashCode))
                        CashCode = await NodeContext.Cash_CodeLookup
                                                .Where(c => c.CashTypeCode < (short)NodeEnum.CashType.Bank)
                                                .OrderBy(c => c.CashCode)
                                                .Select(c => c.CashCode)
                                                .FirstAsync();

                    CashDescription = await NodeContext.Cash_tbCodes.Where(c => c.CashCode == CashCode).Select(c => c.CashDescription).FirstOrDefaultAsync();

                    var taxDescriptions = from t in NodeContext.App_TaxCodes
                                          orderby t.TaxDescription
                                          select t.TaxDescription;

                    TaxDescriptions = new SelectList(await taxDescriptions.ToListAsync());

                    if (!string.IsNullOrEmpty(taxCode))
                        TaxCode = taxCode;
                    else if (!string.IsNullOrEmpty(accountCode) && await NodeContext.Subject_tbSubjects.Where(o => o.SubjectCode == accountCode).Select(o => o.TaxCode).SingleOrDefaultAsync() != null)
                        TaxCode = await NodeContext.Subject_tbSubjects.Where(o => o.SubjectCode == accountCode).Select(o => o.TaxCode).SingleAsync();
                    else if (!string.IsNullOrEmpty(cashCode))
                        TaxCode = await NodeContext.Cash_tbCodes.Where(c => c.CashCode == cashCode).Select(c => c.TaxCode).SingleOrDefaultAsync();
                    else if (string.IsNullOrEmpty(TaxCode) && !string.IsNullOrEmpty(CashCode))
                    {
                        CashCodes cash = new(NodeContext, CashCode);
                        TaxCode = cash.TaxCode;
                    }
                    else
                        TaxCode = await NodeContext.App_tbTaxCodes
                                            .Where(t => t.TaxTypeCode == (short)NodeEnum.TaxType.VAT)
                                            .OrderBy(t => t.TaxCode)
                                            .Select(t => t.TaxCode)
                                            .FirstAsync();

                    TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();
                }

                Cash_PaymentsUnposted = new Cash_vwPaymentsUnposted
                {
                    AccountCode = AccountCode,
                    SubjectCode = SubjectCode,
                    CashCode = CashCode,
                    TaxCode = TaxCode,
                    PaidOn = DateTime.Today,
                    UserId = await profile.UserId(UserManager.GetUserId(User)),
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User)),
                    IsProfitAndLoss = false
                };

                Cash_PaymentsUnposted.UpdatedBy = Cash_PaymentsUnposted.InsertedBy;

                Subjects orgs = new(NodeContext, SubjectCode);

                var balance = await orgs.BalanceOutstanding();

                if (balance < 0)
                    Cash_PaymentsUnposted.PaidOutValue = Math.Abs(balance);
                else
                    Cash_PaymentsUnposted.PaidInValue = balance;

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
                CashAccounts cashAccounts = new(NodeContext);

                Cash_PaymentsUnposted.AccountCode = await NodeContext.Subject_tbAccounts.Where(t => t.AccountName == AccountName).Select(t => t.AccountCode).FirstAsync();
                Cash_PaymentsUnposted.PaymentCode = await cashAccounts.NextPaymentCode();
                Cash_PaymentsUnposted.SubjectCode = await NodeContext.Subject_tbSubjects.Where(o => o.SubjectName == SubjectName).Select(o => o.SubjectCode).FirstAsync();

                if (InputMode == 1)
                {
                    Cash_PaymentsUnposted.CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstOrDefaultAsync();
                    Cash_PaymentsUnposted.TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstOrDefaultAsync();
                }

                Cash_PaymentsUnposted.UpdatedOn = DateTime.Now;
                Cash_PaymentsUnposted.InsertedOn = DateTime.Now;

                if (!ModelState.IsValid || (Cash_PaymentsUnposted.PaidInValue + Cash_PaymentsUnposted.PaidOutValue == 0)
                        || (Cash_PaymentsUnposted.PaidInValue != 0 && Cash_PaymentsUnposted.PaidOutValue != 0))
                    return Page();

                NodeContext.Cash_PaymentsUnposted.Add(Cash_PaymentsUnposted);
                await NodeContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(ReturnUrl))
                    return RedirectToPage(ReturnUrl);
                else
                {
                    RouteValueDictionary route = new();
                    route.Add("AccountCode", Cash_PaymentsUnposted.AccountCode);

                    return RedirectToPage("./Index", route);
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostNewSubjectCode()
        {
            await SaveSession();    
            return LocalRedirect(@"/Subject/Update/Create?returnUrl=/Cash/PaymentEntry/Create");
        }

        public async Task<IActionResult> OnPostGetSubjectCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Subject/Index?returnUrl=/Cash/PaymentEntry/Create");
        }

        public async Task<IActionResult> OnPostGetCashCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Index?returnUrl=/Cash/PaymentEntry/Create");
        }

        public async Task<IActionResult> OnPostNewCashCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Cash/CashCode/Create?returnUrl=/Cash/PaymentEntry/Create");
        }

        public async Task<IActionResult> OnPostGetTaxCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Tax/TaxCode/Search?returnUrl=/Cash/PaymentEntry/Create");
        }

        public async Task<IActionResult> OnPostNewTaxCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Tax/TaxCode/Create?returnUrl=/Cash/PaymentEntry/Create");
        }


        async Task SaveSession()
        {
            try
            {
                AccountCode = Cash_PaymentsUnposted?.AccountCode;
                SubjectCode = await NodeContext.Subject_tbSubjects.Where(o => o.SubjectName == SubjectName).Select(o => o.SubjectCode).FirstAsync();
                CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstAsync();
                TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();
            }
            catch
            {

            }
        }

    }
}
