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

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class CreateItemModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_vwRegister Invoice_Header { get; set; }

        [BindProperty]
        public Invoice_tbItem Invoice_Item { get; set; }

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

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateItemModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber)
        {
            try
            {
                if (invoiceNumber == null)
                    return NotFound();

                Invoice_Header = await NodeContext.Invoice_Register.FirstOrDefaultAsync(m => m.InvoiceNumber == invoiceNumber);

                if (Invoice_Header == null)
                    return NotFound();
                else
                {
                    var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                    if (!isAuthorized)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        string userId = await profile.UserId(user.Id);

                        if (userId != Invoice_Header.UserId)
                            return Forbid();
                    }

                    NodeEnum.CashPolarity cashMode = NodeEnum.CashPolarity.Neutral;
                    
                    switch ((NodeEnum.InvoiceType)Invoice_Header.InvoiceTypeCode)
                    {
                        case NodeEnum.InvoiceType.SalesInvoice:
                            cashMode = NodeEnum.CashPolarity.Income;
                            break;
                        case NodeEnum.InvoiceType.CreditNote:
                            cashMode = NodeEnum.CashPolarity.Income;
                            break;
                        case NodeEnum.InvoiceType.PurchaseInvoice:
                            cashMode = NodeEnum.CashPolarity.Expense;
                            break;
                        case NodeEnum.InvoiceType.DebitNote:
                            cashMode = NodeEnum.CashPolarity.Expense;
                            break;

                    };

                    var cashDescriptions = from t in NodeContext.Cash_CodeLookup
                                           where t.CashTypeCode < (short)NodeEnum.CashType.Bank && t.CashPolarityCode == (short)cashMode
                                           orderby t.CashDescription
                                           select t.CashDescription;

                    CashDescriptions = new SelectList(await cashDescriptions.ToListAsync());

                    string cashCode = await NodeContext.Cash_CodeLookup
                                                .Where(c => c.CashTypeCode < (short)NodeEnum.CashType.Bank && c.CashPolarityCode == (short)cashMode)
                                                .OrderBy(c => c.CashCode)
                                                .Select(c => c.CashCode)
                                                .FirstAsync();

                    CashDescription = await NodeContext.Cash_tbCodes.Where(c => c.CashCode == cashCode).Select(c => c.CashDescription).FirstOrDefaultAsync();

                    var taxDescriptions = from t in NodeContext.App_TaxCodes
                                          orderby t.TaxDescription
                                          select t.TaxDescription;

                    TaxDescriptions = new SelectList(await taxDescriptions.ToListAsync());

                    string taxCode = await NodeContext.App_tbTaxCodes
                                            .Where(t => t.TaxTypeCode == (short)NodeEnum.TaxType.VAT)
                                            .OrderBy(t => t.TaxCode)
                                            .Select(t => t.TaxCode)
                                            .FirstAsync();

                    TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == taxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();

                    Invoice_Item = new()
                    {
                        InvoiceNumber = invoiceNumber,
                        TotalValue = 0,
                        InvoiceValue = 0,
                        TaxValue = 0,
                        TaxCode = taxCode,
                        CashCode = cashCode
                    };

                    await SetViewData();

                    return Page();
                }
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

                if (Invoice_Item.TotalValue != 0 && Invoice_Item.InvoiceValue != 0)
                    Invoice_Item.InvoiceValue = 0;

                Invoice_Item.CashCode = await NodeContext.Cash_tbCodes.Where(c => c.CashDescription == CashDescription).Select(c => c.CashCode).FirstAsync();
                Invoice_Item.TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();

                NodeContext.Invoice_tbItems.Add(Invoice_Item);
                await NodeContext.SaveChangesAsync();

                Invoices invoices = new(NodeContext, Invoice_Item.InvoiceNumber);
                await invoices.Accept();

                var invoiceHeader = await NodeContext.Invoice_tbInvoices.Where(i => i.InvoiceNumber == Invoice_Item.InvoiceNumber).FirstAsync();

                FinancialPeriods periods = new(NodeContext);
                if (invoiceHeader.InvoicedOn < periods.ActiveStartOn)
                    await periods.Generate();

                Subjects orgs = new(NodeContext, invoiceHeader.SubjectCode);
                await orgs.Rebuild();

                RouteValueDictionary route = new();
                route.Add("InvoiceNumber", Invoice_Item.InvoiceNumber);

                return RedirectToPage("./Edit", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}