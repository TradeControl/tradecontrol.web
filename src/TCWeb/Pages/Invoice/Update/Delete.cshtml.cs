using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class DeleteModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_vwRegister Invoice_Header { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public DeleteModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
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

        public async Task<IActionResult> OnPostAsync(string invoiceNumber)
        {
            try
            {
                if (invoiceNumber == null)
                    return NotFound();

                var invoice = await NodeContext.Invoice_tbInvoices.FindAsync(invoiceNumber);

                if (invoice != null)
                {
                    NodeContext.Invoice_tbInvoices.Remove(invoice);
                    await NodeContext.SaveChangesAsync();

                    Invoices invoices = new(NodeContext);
                    if (await invoices.CancelPending(invoice.UserId))
                    {
                        Subjects orgs = new(NodeContext, invoice.AccountCode);
                        await orgs.Rebuild();
                        return RedirectToPage("./Index");
                    }
                    else
                        throw new Exception($"Invoice {invoiceNumber} failed cancellation request.");
                }
                else
                    return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
