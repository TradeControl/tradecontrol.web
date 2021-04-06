using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using TradeControl.Web.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Users
{
    public class RoleModel : DI_BasePageModel
    {
        public RoleModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public AspNet_UserRegistration AspNet_UserRegistration { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
                return NotFound();

            AspNet_UserRegistration = await NodeContext.AspNet_UserRegistrations.FirstOrDefaultAsync(m => m.Id == id);

            if (AspNet_UserRegistration == null)
                return NotFound();
            else if (!AspNet_UserRegistration.IsConfirmed)
                return RedirectToPage("./Index");
            else
            {
                var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                          User, AspNet_UserRegistration,
                                          Operations.Update);
                if (!isAuthorized.Succeeded)
                    return Forbid();

                await SetViewData();

                return Page();
            }

        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
                return NotFound();

            var user = await UserManager.FindByIdAsync(id);
            
            if (AspNet_UserRegistration.IsAdministrator)
            {
                if (!await UserManager.IsInRoleAsync(user, Constants.AdministratorsRole))
                    await UserManager.AddToRoleAsync(user, Constants.AdministratorsRole);
            }
            else if (await UserManager.IsInRoleAsync(user, Constants.AdministratorsRole) && (User.Identity.Name != user.UserName))
                await UserManager.RemoveFromRoleAsync(user, Constants.AdministratorsRole);

            if (AspNet_UserRegistration.IsManager)
            {
                if (!await UserManager.IsInRoleAsync(user, Constants.ManagersRole))
                    await UserManager.AddToRoleAsync(user, Constants.ManagersRole);
            }
            else if (await UserManager.IsInRoleAsync(user, Constants.ManagersRole))
                await UserManager.RemoveFromRoleAsync(user, Constants.ManagersRole);


            return RedirectToPage("./Index");
        }
    }
}
