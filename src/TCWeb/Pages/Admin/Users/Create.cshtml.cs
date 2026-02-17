using System;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace TradeControl.Web.Pages.Admin.Users
{
    public class CreateModel : DI_BasePageModel
    {
        IAuthorizationService AuthorizationService { get; }
        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            AuthorizationService = authorizationService;
            UserManager = userManager;
        }

        [BindProperty]
        public Usr_tbUser Usr_tbUser { get; set; }

        public SelectList CalendarCodes { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                if (id == null)
                    return NotFound();
                else
                {
                    var AspNet_UserRegistration = await NodeContext.AspNet_UserRegistrations.FirstOrDefaultAsync(m => m.Id == id);

                    if (AspNet_UserRegistration == null || AspNet_UserRegistration.IsRegistered)
                        return NotFound();
                    else
                    {
                        var isAuthorized = await AuthorizationService.AuthorizeAsync(User, AspNet_UserRegistration, Operations.Approve);

                        if (!isAuthorized.Succeeded)
                            return Forbid();

                        Profile profile = new(NodeContext);
                        var userName = await profile.UserName(UserManager.GetUserId(User));

                        var calendarCodes = NodeContext.App_tbCalendars.OrderBy(c => c.CalendarCode).Select(c => c.CalendarCode);
                        CalendarCodes = new SelectList(await calendarCodes.ToListAsync());

                        Usr_tbUser = new() {
                            CalendarCode = await calendarCodes.FirstOrDefaultAsync(),
                            EmailAddress = AspNet_UserRegistration.EmailAddress,
                            LogonName = AspNet_UserRegistration.EmailAddress,
                            IsEnabled = -1,
                            InsertedBy = userName,
                            UpdatedBy = userName,
                            InsertedOn = DateTime.Now,
                            UpdatedOn = DateTime.Now
                        };
                    }
                }

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<JsonResult> OnGetDefaultUserIdAsync(string userName)
        {
            try
            {
                userName ??= string.Empty;
                userName = userName.Trim();

                if (string.IsNullOrWhiteSpace(userName))
                    return new JsonResult(new { ok = true, userId = string.Empty });

                var userId = await NodeContext.UserIdDefault(userName);
                return new JsonResult(new { ok = true, userId });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { ok = false, userId = string.Empty });
            }
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                if (!ModelState.IsValid)
                    return Page();

                var embedded = Request.Query.TryGetValue("embedded", out var emb) && emb == "1";
                var returnNode = Request.Query.TryGetValue("returnNode", out var rn) ? rn.ToString() : "Users";

                Usr_tbUser.UserId = (Usr_tbUser.UserId ?? string.Empty).Trim();
                Usr_tbUser.UserName = (Usr_tbUser.UserName ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(Usr_tbUser.UserId) && !string.IsNullOrWhiteSpace(Usr_tbUser.UserName))
                    Usr_tbUser.UserId = await NodeContext.UserIdDefault(Usr_tbUser.UserName);

                NodeContext.Usr_tbUsers.Add(Usr_tbUser);
                await NodeContext.SaveChangesAsync();

                var user = await UserManager.FindByIdAsync(id);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                await UserManager.ConfirmEmailAsync(user, code);

                if (Usr_tbUser.IsAdministrator && !await UserManager.IsInRoleAsync(user, Constants.AdministratorsRole))
                    await UserManager.AddToRoleAsync(user, Constants.AdministratorsRole);

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
