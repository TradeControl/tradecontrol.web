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
    public class CreateModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_tbContact Org_tbContact { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        public CreateModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode))
                    return NotFound();

                var org = await NodeContext.Org_tbOrgs.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

                if (org == null)
                    return NotFound();
                else
                    AccountName = org.AccountName;

                Profile profile = new(NodeContext);
                Org_tbContact = new()
                {
                    AccountCode = org.AccountCode,
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User)),
                    InsertedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };

                Org_tbContact.UpdatedBy = Org_tbContact.InsertedBy;

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

                NodeContext.Org_tbContacts.Add(Org_tbContact);

                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("AccountCode", Org_tbContact.AccountCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
