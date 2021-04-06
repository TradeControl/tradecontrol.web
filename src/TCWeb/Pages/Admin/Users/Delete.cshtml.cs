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
using TradeControl.Web.Models;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;

namespace TradeControl.Web.Pages.Admin.Users
{
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context,
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
            {
                return NotFound();
            }

            AspNet_UserRegistration = await NodeContext.AspNet_UserRegistrations.FirstOrDefaultAsync(m => m.Id == id);

            if (AspNet_UserRegistration == null)
                return NotFound();
            {
                var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                          User, AspNet_UserRegistration,
                                          Operations.Delete);
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
            await UserManager.DeleteAsync(user);

            return RedirectToPage("./Index");
        }
    }
}
