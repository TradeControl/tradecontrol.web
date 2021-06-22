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
        IAuthorizationService AuthorizationService { get; }
        UserManager<TradeControlWebUser> UserManager { get; }

        public DeleteModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            AuthorizationService = authorizationService;
            UserManager = userManager;
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
                else
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
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
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
                await UserManager.DeleteAsync(user);

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
