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
    public class CreateModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_tbAddress Org_tbAddress { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        [BindProperty]
        [Display(Name = "Admin Address?")]
        public bool IsAdminAddress { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

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
                Orgs orgs = new(NodeContext, accountCode);

                Org_tbAddress = new()
                {
                    AccountCode = accountCode,
                    AddressCode = await orgs.NextAddressCode(),
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User)),
                    InsertedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };

                Org_tbAddress.UpdatedBy = Org_tbAddress.InsertedBy;

                IsAdminAddress = !(await NodeContext.Org_tbAddresses.Where(t => t.AccountCode == accountCode).AnyAsync());

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

                NodeContext.Org_tbAddresses.Add(Org_tbAddress);
                if (IsAdminAddress)
                {
                    Org_tbOrg org = await NodeContext.Org_tbOrgs.FirstOrDefaultAsync(t => t.AccountCode == Org_tbAddress.AccountCode);
                    org.AddressCode = Org_tbAddress.AddressCode;
                }

                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("accountCode", Org_tbAddress.AccountCode);

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
