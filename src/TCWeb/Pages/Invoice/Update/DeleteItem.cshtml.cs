using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class DeleteItemModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_vwRegisterDetail Invoice_Detail { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public DeleteItemModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber, string cashCode)
        {
            try
            {
                if (invoiceNumber == null || cashCode == null)
                    return NotFound();

                Invoice_Detail = await NodeContext.Invoice_RegisterDetails.FirstOrDefaultAsync(m => m.InvoiceNumber == invoiceNumber && m.CashCode == cashCode);

                if (Invoice_Detail == null)
                    return NotFound();
                else
                {
                    //if (await NodeContext.Invoice_RegisterDetails.Where(t => t.InvoiceNumber == invoiceNumber).CountAsync() <= 1)
                    //    return Forbid();

                    var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                    if (!isAuthorized)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        string userId = await profile.UserId(user.Id);

                        if (userId != Invoice_Detail.UserId)
                            return Forbid();
                    }

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

        public async Task<IActionResult> OnPostAsync(string invoiceNumber, string cashCode)
        {
            try
            {
                if (invoiceNumber == null || cashCode == null)
                    return NotFound();

                var invoiceEntry = await NodeContext.Invoice_tbItems.Where(t => t.InvoiceNumber == invoiceNumber && t.CashCode == cashCode).FirstAsync();

                if (invoiceEntry != null)
                {
                    NodeContext.Invoice_tbItems.Remove(invoiceEntry);
                    await NodeContext.SaveChangesAsync();
                }

                Invoices invoices = new(NodeContext, invoiceNumber);
                await invoices.Accept();

                var invoiceHeader = await NodeContext.Invoice_tbInvoices.Where(i => i.InvoiceNumber == invoiceNumber).FirstAsync();

                FinancialPeriods periods = new(NodeContext);
                if (invoiceHeader.InvoicedOn < periods.ActiveStartOn)
                    await periods.Generate();

                Subjects orgs = new(NodeContext, invoiceHeader.AccountCode);
                await orgs.Rebuild();

                RouteValueDictionary route = new();
                route.Add("InvoiceNumber", invoiceNumber);

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

