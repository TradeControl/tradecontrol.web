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


namespace TradeControl.Web.Pages.Subject.Contact
{
    [Authorize(Roles = "Administrators, Managers")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_tbContact Subject_tbContact { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode, string contactName)
        {
            try
            {
                if (accountCode == null || contactName == null)
                    return NotFound();

                var subject = await NodeContext.Subject_tbSubjects.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

                if (subject == null)
                    return NotFound();
                else
                    AccountName = subject.AccountName;

                Subject_tbContact = await NodeContext.Subject_tbContacts.FirstOrDefaultAsync(m => m.AccountCode == accountCode && m.ContactName == contactName);

                if (Subject_tbContact == null)
                    return NotFound();
                else
                {
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

                Profile profile = new(NodeContext);
                Subject_tbContact.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

                NodeContext.Attach(Subject_tbContact).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Subject_tbContacts.AnyAsync(e => e.AccountCode == Subject_tbContact.AccountCode && e.ContactName == Subject_tbContact.ContactName))
                        return NotFound();
                    else
                        throw;

                }

                RouteValueDictionary route = new();
                route.Add("accountCode", Subject_tbContact.AccountCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
