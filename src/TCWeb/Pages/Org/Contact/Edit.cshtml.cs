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


namespace TradeControl.Web.Pages.Org.Contact
{
    [Authorize(Roles = "Administrators, Managers")]
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        [BindProperty]
        public Org_tbContact Org_tbContact { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        public async Task<IActionResult> OnGetAsync(string accountCode, string contactName)
        {
            if (accountCode == null || contactName == null)
                return NotFound();
            
            var org = await NodeContext.Org_tbOrgs.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

            if (org == null)
                return NotFound();
            else
                AccountName = org.AccountName;

            Org_tbContact = await NodeContext.Org_tbContacts.FirstOrDefaultAsync(m => m.AccountCode == accountCode && m.ContactName == contactName);

            if (Org_tbContact == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Profile profile = new(NodeContext);
            Org_tbContact.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

            NodeContext.Attach(Org_tbContact).State = EntityState.Modified;

            try
            {
                await NodeContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await NodeContext.Org_tbContacts.AnyAsync(e => e.AccountCode == Org_tbContact.AccountCode && e.ContactName == Org_tbContact.ContactName))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            RouteValueDictionary route = new();
            route.Add("accountCode", Org_tbContact.AccountCode);

            return RedirectToPage("./Index", route);
        }

    }
}
