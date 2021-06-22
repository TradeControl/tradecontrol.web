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

namespace TradeControl.Web.Pages.Org.Address
{
    [Authorize(Roles = "Administrators, Managers")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_tbAddress Org_tbAddress { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        [BindProperty]
        [Display(Name = "Admin Address?")]
        public bool IsAdminAddress { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string addressCode)
        {
            try
            {
                if (string.IsNullOrEmpty(addressCode))
                    return NotFound();

                Org_tbAddress = await NodeContext.Org_tbAddresses.FirstOrDefaultAsync(t => t.AddressCode == addressCode);

                if (Org_tbAddress == null)
                    return NotFound();

                var org = await NodeContext.Org_tbOrgs.FirstAsync(t => t.AccountCode == Org_tbAddress.AccountCode);
                AccountName = org.AccountName;
                IsAdminAddress = org.AddressCode == addressCode;

                await SetViewData();
                return Page();
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
                Org_tbAddress.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

                NodeContext.Attach(Org_tbAddress).State = EntityState.Modified;

                if (IsAdminAddress)
                {
                    Org_tbOrg org = await NodeContext.Org_tbOrgs.FirstOrDefaultAsync(t => t.AccountCode == Org_tbAddress.AccountCode);
                    if (org.AddressCode != Org_tbAddress.AddressCode)
                    {
                        org.AddressCode = Org_tbAddress.AddressCode;
                        NodeContext.Attach(org).State = EntityState.Modified;
                    }
                }

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Org_tbAddresses.AnyAsync(e => e.AddressCode == Org_tbAddress.AddressCode))
                        return NotFound();
                    else
                        throw;

                }

                RouteValueDictionary route = new();
                route.Add("accountCode", Org_tbAddress.AccountCode);

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
