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

namespace TradeControl.Web.Pages.Invoice.Raise
{
    public class CreateModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_tbEntry Invoice_Entry { get; set; }

        public SelectList OrganisationNames { get; set; }
        [BindProperty]
        [Required]
        [Display(Name = "Organisation")]
        public string OrganisationName { get; set; }

        public SelectList CashDescriptions { get; set; }
        [BindProperty]
        [Required]
        [Display(Name = "Cash Code")]
        public string CashDescription { get; set; }

        public SelectList TaxDescriptions { get; set; }
        [BindProperty]
        [Required]
        [Display(Name = "Tax Code")]
        public string TaxDescription { get; set; }

        [BindProperty]
        public string InvoiceType { get; set; }
        public SelectList InvoiceTypes { get; set; }

        #region session data
        const string SessionKeyAccountCode = "_AccountCode";
        const string SessionKeyCashCode = "_CashCode";
        const string SessionKeyTaxCode = "_TaxCode";
        const string SessionKeyInvoiceTypeCode = "_InvoiceTypeCode";

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

        NodeEnum.InvoiceType InvoiceTypeCode
        {
            get
            {
                try
                {
                    var invoiceType = HttpContext.Session.GetInt32(SessionKeyInvoiceTypeCode);
                    return (NodeEnum.InvoiceType)invoiceType;
                }
                catch
                {
                    InvoiceTypeCode = NodeEnum.InvoiceType.SalesInvoice;
                    return NodeEnum.InvoiceType.SalesInvoice;
                }
            }
            set
            {
                int invoiceType = (int)value;
                HttpContext.Session.SetInt32(SessionKeyInvoiceTypeCode, invoiceType);
            }
        }
        #endregion

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode, string cashCode, string taxCode)
        {
            try
            {
                var organisationNames = from t in NodeContext.Org_AccountLookup
                                        orderby t.AccountName
                                        select t.AccountName;

                OrganisationNames = new SelectList(await organisationNames.ToListAsync());

                var profile = new Profile(NodeContext);

                if (!string.IsNullOrEmpty(accountCode))
                    AccountCode = accountCode;
                else if (string.IsNullOrEmpty(AccountCode))
                    AccountCode = await profile.CompanyAccountCode();

                OrganisationName = await NodeContext.Org_tbOrgs.Where(o => o.AccountCode == AccountCode).Select(o => o.AccountName).FirstOrDefaultAsync();

                var cashDescriptions = from t in NodeContext.Cash_CodeLookup
                                       where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                       orderby t.CashDescription
                                       select t.CashDescription;

                CashDescriptions = new SelectList(await cashDescriptions.ToListAsync());

                if (!string.IsNullOrEmpty(cashCode))
                {
                    CashCodes code = new(NodeContext, cashCode);
                    CashCode = cashCode;
                    TaxCode = code.TaxCode;
                }
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
                else if (string.IsNullOrEmpty(TaxCode))
                    TaxCode = await NodeContext.App_tbTaxCodes
                                        .Where(t => t.TaxTypeCode == (short)NodeEnum.TaxType.VAT)
                                        .OrderBy(t => t.TaxCode)
                                        .Select(t => t.TaxCode)
                                        .FirstAsync();

                TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();

                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());
                InvoiceType = await NodeContext.Invoice_tbTypes.Where(t => t.InvoiceTypeCode == (short)InvoiceTypeCode).Select(t => t.InvoiceType).FirstAsync();

                Invoice_Entry = new()
                {
                    UserId = await profile.UserId(UserManager.GetUserId(User)),
                    AccountCode = AccountCode,
                    InvoiceTypeCode = (short)InvoiceTypeCode,
                    CashCode = CashCode,
                    TaxCode = TaxCode,
                    InvoicedOn = DateTime.Today
                };

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
                Invoice_Entry.InvoiceTypeCode = await NodeContext.Invoice_tbTypes.Where(t => t.InvoiceType == InvoiceType).Select(t => t.InvoiceTypeCode).FirstAsync();

                if (!ModelState.IsValid || (Invoice_Entry.TotalValue + Invoice_Entry.InvoiceValue == 0))
                    return Page();

                if (Invoice_Entry.TotalValue != 0 && Invoice_Entry.InvoiceValue != 0)
                    Invoice_Entry.InvoiceValue = 0;

                Invoice_Entry.AccountCode = await NodeContext.Org_tbOrgs.Where(o => o.AccountName == OrganisationName).Select(o => o.AccountCode).FirstAsync();
                Invoice_Entry.CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstAsync();
                Invoice_Entry.TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();

                NodeContext.Invoice_tbEntries.Add(Invoice_Entry);
                await NodeContext.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostNewAccountCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Org/Update/Create?returnUrl=/Invoice/Raise/Create");
        }

        public async Task<IActionResult> OnPostGetAccountCode()
        {
            await SaveSession();
            return LocalRedirect(@"/Org/Index?returnUrl=/Invoice/Raise/Create");
        }

        public async Task<IActionResult> OnPostGetCashCode()
        {
            await SaveSession ();
            return LocalRedirect(@"/Cash/CashCode/Index?returnUrl=/Invoice/Raise/Create");
        }

        public async Task<IActionResult> OnPostNewCashCode()
        {
            await SaveSession ();
            return LocalRedirect(@"/Cash/CashCode/Create?returnUrl=/Invoice/Raise/Create");
        }

        public async Task<IActionResult> OnPostGetTaxCode()
        {
            await SaveSession ();
            return LocalRedirect(@"/Tax/TaxCode/Search?returnUrl=/Invoice/Raise/Create&TaxType=VAT");
        }

        public async Task<IActionResult> OnPostNewTaxCode()
        {
            await SaveSession ();
            return LocalRedirect(@"/Tax/TaxCode/Create?returnUrl=/Invoice/Raise/Create");
        }

        async Task SaveSession()
        {
            try
            {
                InvoiceTypeCode = (NodeEnum.InvoiceType)await NodeContext.Invoice_tbTypes.Where(t => t.InvoiceType == InvoiceType).Select(t => t.InvoiceTypeCode).FirstAsync();
                AccountCode = await NodeContext.Org_tbOrgs.Where(o => o.AccountName == OrganisationName).Select(o => o.AccountCode).FirstAsync(); 
                CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstAsync();
                TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();
            }
            catch
            {

            }
        }
    }
}
