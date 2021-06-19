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
        }

        [BindProperty]
        public Usr_tbUser Usr_tbUser { get; set;  }

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
                        var isAuthorized = await AuthorizationService.AuthorizeAsync(
                          User, AspNet_UserRegistration,
                          Operations.Approve);
                        if (!isAuthorized.Succeeded)
                            return Forbid();

                        Profile profile = new(NodeContext);
                        var userName = await profile.UserName(UserManager.GetUserId(User));

                        var calendarCodes = NodeContext.App_tbCalendars.OrderBy(c => c.CalendarCode).Select(c => c.CalendarCode);
                        CalendarCodes = new SelectList(await calendarCodes.ToListAsync());

                        Usr_tbUser = new()
                        {
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

                if (!ModelState.IsValid)
                    return Page();

                NodeContext.Usr_tbUsers.Add(Usr_tbUser);
                await NodeContext.SaveChangesAsync();

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
