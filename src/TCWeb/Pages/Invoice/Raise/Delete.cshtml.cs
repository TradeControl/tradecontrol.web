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

namespace TradeControl.Web.Pages.Invoice.Raise
{
    public class DeleteModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_vwEntry Entry { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public DeleteModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
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
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string accountCode, string cashCode)
        {
            try
            {
                if (accountCode == null || cashCode == null)
                    return NotFound();

                var invoiceEntry = await NodeContext.Invoice_tbEntries.Where(t => t.AccountCode == accountCode && t.CashCode == cashCode).FirstAsync();

                if (invoiceEntry != null)
                {
                    NodeContext.Invoice_tbEntries.Remove(invoiceEntry);
                    await NodeContext.SaveChangesAsync();
                }

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
