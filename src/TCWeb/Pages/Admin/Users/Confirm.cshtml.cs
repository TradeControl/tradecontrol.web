using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Authorization;
using TradeControl.Web.Areas.Identity.Data;

namespace TradeControl.Web.Pages.Admin.Users
{
    public class ConfirmModel : DI_BasePageModel
    {
        IAuthorizationService AuthorizationService { get; }
        UserManager<TradeControlWebUser> UserManager { get; }

        public ConfirmModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context)
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

                var embedded = Request.Query.TryGetValue("embedded", out var emb) && emb == "1";
                var returnNode = Request.Query.TryGetValue("returnNode", out var rn) ? rn.ToString() : "Users";

                var user = await UserManager.FindByIdAsync(id);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                await UserManager.ConfirmEmailAsync(user, code);

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
