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

namespace TradeControl.Web.Pages.Invoice.Raise
{
    public class PostModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_vwEntry Entry { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public PostModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode, string cashCode)
        {
            try
            {
                if (accountCode == null || cashCode == null)
                    return NotFound();

                Entry = await NodeContext.Invoice_Entries.FirstOrDefaultAsync(m => m.AccountCode == accountCode && m.CashCode == cashCode);

                if (Entry == null)
                    return NotFound();
                else
                {
                    var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                    if (!isAuthorized)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        string userId = await profile.UserId(user.Id);

                        if (userId != Entry.UserId)
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

        public async Task<IActionResult> OnPostEntry()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                return await PostInvoice(Entry.AccountCode, Entry.CashCode);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAccount()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                return await PostInvoice(Entry.AccountCode);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task<IActionResult> PostInvoice(string accountCode)
        {
            return await PostInvoice(accountCode, string.Empty);
        }

        private async Task<IActionResult> PostInvoice(string accountCode, string cashCode)
        {
            try
            {
                Invoices invoices = new(NodeContext);

                bool success = true;
                bool isEmailed = false;

                isEmailed = Entry.InvoiceTypeCode == (short)NodeEnum.InvoiceType.SalesInvoice || Entry.InvoiceTypeCode == (short)NodeEnum.InvoiceType.CreditNote;

                if (!string.IsNullOrEmpty(cashCode))
                    success = await invoices.PostByEntry(Entry.UserId, accountCode, cashCode);
                else
                    success = await invoices.PostByAccount(Entry.UserId, accountCode);

                if (success)
                {

                    if (isEmailed)
                    {
                        RouteValueDictionary route = new();
                        route.Add("Printed", false);
                        return RedirectToPage("../Update/Index", route);
                    }
                    else
                        return RedirectToPage("./Index");
                }
                else
                    throw new Exception("Unable to raise invoices due to errors.");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
