using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Authorization;
using TradeControl.Web.Areas.Identity.Data;

namespace TradeControl.Web.Pages.Admin.Users
{
    public class ConfirmModel : DI_BasePageModel
    {
        public ConfirmModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public AspNet_UserRegistration AspNet_UserRegistration { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                AspNet_UserRegistration = await NodeContext.AspNet_UserRegistrations.FirstOrDefaultAsync(m => m.Id == id);

                if (AspNet_UserRegistration == null)
                    return NotFound();
                else if (AspNet_UserRegistration.IsConfirmed || !AspNet_UserRegistration.IsRegistered)
                    return RedirectToPage("./Index");
                else
                {
                    var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                              User, AspNet_UserRegistration,
                                              Operations.Approve);
                    if (!isAuthorized.Succeeded)
                        return Forbid();

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

        public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                var user = await UserManager.FindByIdAsync(id);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                await UserManager.ConfirmEmailAsync(user, code);

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
