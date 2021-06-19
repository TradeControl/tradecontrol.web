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

namespace TradeControl.Web.Pages.Invoice.Raise
{
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Invoice_vwEntry Entry_Header { get; set; }

        [BindProperty]
        public Invoice_tbEntry Invoice_Entry { get; set; }

        public SelectList TaxDescriptions { get; set; }
        [BindProperty]
        public string TaxDescription { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode, string cashCode)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode) || string.IsNullOrEmpty(cashCode))
                    return NotFound();

                Entry_Header = await NodeContext.Invoice_Entries.FirstOrDefaultAsync(m => m.AccountCode == accountCode && m.CashCode == cashCode);

                if (Entry_Header == null)
                    return NotFound();

                Invoice_Entry = await NodeContext.Invoice_tbEntries.FirstOrDefaultAsync(m => m.AccountCode == accountCode && m.CashCode == cashCode);

                if ((User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole)) == false)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    if (Invoice_Entry.UserId != await profile.UserId(user.Id))
                        return Forbid();
                }

                var taxDescriptions = from t in NodeContext.App_TaxCodes
                                      orderby t.TaxDescription
                                      select t.TaxDescription;

                TaxDescriptions = new SelectList(await taxDescriptions.ToListAsync());
                TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == Invoice_Entry.TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();

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

                Invoice_Entry.TaxCode = await NodeContext.App_tbTaxCodes.Where(c => c.TaxDescription == TaxDescription).Select(c => c.TaxCode).FirstAsync();

                NodeContext.Attach(Invoice_Entry).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Invoice_Entries.AnyAsync(e => e.AccountCode == Invoice_Entry.AccountCode && e.CashCode == Invoice_Entry.CashCode))
                        return NotFound();
                    else
                        throw;

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
