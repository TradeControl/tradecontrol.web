using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Authorization;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

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

                var embedded = Request.Query.TryGetValue("embedded", out var emb) && emb == "1";
                var returnNode = Request.Query.TryGetValue("returnNode", out var rn) ? rn.ToString() : "Users";

                var user = await UserManager.FindByIdAsync(id);
                var email = user?.Email?.Trim();

                if (!string.IsNullOrWhiteSpace(email))
                {
                    var tbUser = await NodeContext.Usr_tbUsers
                        .Where(u => u.EmailAddress == email)
                        .SingleOrDefaultAsync();

                    if (tbUser != null)
                    {
                        try
                        {
                            NodeContext.Usr_tbUsers.Remove(tbUser);
                            await NodeContext.SaveChangesAsync();

                            await NodeContext.EventLog(
                                NodeEnum.EventType.IsInformation,
                                $"User account '{tbUser.UserId}' deleted from Usr.tbUser as part of Identity user deletion. Email='{email}'.");
                        }
                        catch (Exception ex)
                        {
                            await NodeContext.ErrorLog(ex);

                            await NodeContext.EventLog(
                                NodeEnum.EventType.IsWarning,
                                $"Identity user deletion blocked: cannot delete Usr.tbUser for Email='{email}' due to references. " +
                                "Remove dependent records first, then retry the deletion.");

                            return embedded
                                ? Redirect("/Admin/EventLog/Index?embedded=1&returnNode=Users")
                                : RedirectToPage("/Admin/EventLog/Index");
                        }
                    }
                }

                if (user != null)
                    await UserManager.DeleteAsync(user);

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
