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

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class EditItemModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_vwRegisterDetail Invoice_Detail { get; set; }

        [BindProperty]
        public Invoice_tbItem Invoice_Item { get; set; }

        public SelectList TaxDescriptions { get; set; }
        [BindProperty]
        public string TaxDescription { get; set; }


        public EditItemModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber, string cashCode)
        {
            try
            {
                if (string.IsNullOrEmpty(invoiceNumber) || string.IsNullOrEmpty(cashCode))
                    return NotFound();

                Invoice_Detail = await NodeContext.Invoice_RegisterDetails.FirstOrDefaultAsync(m => m.InvoiceNumber == invoiceNumber && m.CashCode == cashCode);

                if (Invoice_Detail == null)
                    return NotFound();

                Invoice_Item = await NodeContext.Invoice_tbItems.FirstOrDefaultAsync(m => m.InvoiceNumber == invoiceNumber && m.CashCode == cashCode);

                if ((User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole)) == false)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    if (Invoice_Detail.UserId != await profile.UserId(user.Id))
                        return Forbid();
                }

                var taxDescriptions = from t in NodeContext.App_TaxCodes
                                      orderby t.TaxDescription
                                      select t.TaxDescription;

                TaxDescriptions = new SelectList(await taxDescriptions.ToListAsync());
                TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == Invoice_Item.TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();

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

                var previousItemValue = await NodeContext.Invoice_tbItems.Where(m => m.InvoiceNumber == Invoice_Item.InvoiceNumber && m.CashCode == Invoice_Item.CashCode)
                        .Select(i => i.InvoiceValue + i.TotalValue).FirstAsync();
 
                Invoice_Item.TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();

                var invoiceHeader = await NodeContext.Invoice_tbInvoices.Where(i => i.InvoiceNumber == Invoice_Item.InvoiceNumber).FirstAsync();

                bool orgRebuild = (previousItemValue != Invoice_Item.InvoiceValue + Invoice_Item.TotalValue);

                FinancialPeriods periods = new(NodeContext);
                bool periodRebuild = (invoiceHeader.InvoicedOn < periods.ActiveStartOn);

                NodeContext.Attach(Invoice_Item).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Invoice_tbItems.AnyAsync(e => e.InvoiceNumber == Invoice_Item.InvoiceNumber && e.CashCode == Invoice_Item.CashCode))
                        return NotFound();
                    else
                        throw;

                }

                Invoices invoices = new(NodeContext, Invoice_Item.InvoiceNumber);

                if (await invoices.Accept())
                {
                    if (orgRebuild)
                    {
                        Orgs orgs = new(NodeContext, invoiceHeader.AccountCode);
                        await orgs.Rebuild();
                    }
                    if (periodRebuild)
                        await periods.Generate();
                }

                RouteValueDictionary route = new();
                route.Add("InvoiceNumber", Invoice_Item.InvoiceNumber);

                return RedirectToPage("./Edit", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
