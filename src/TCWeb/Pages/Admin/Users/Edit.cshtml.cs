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
    public class EditModel : DI_BasePageModel
    {
        IAuthorizationService AuthorizationService { get; }
        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context)
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

                    if (AspNet_UserRegistration == null || !AspNet_UserRegistration.IsRegistered)
                        return NotFound();
                    else
                    {
                        var isAuthorized = await AuthorizationService.AuthorizeAsync(User, AspNet_UserRegistration, Operations.Update);

                        if (!isAuthorized.Succeeded)
                            return Forbid();

                        var calendarCodes = NodeContext.App_tbCalendars.OrderBy(c => c.CalendarCode).Select(c => c.CalendarCode);
                        CalendarCodes = new SelectList(await calendarCodes.ToListAsync());

                        Profile profile = new(NodeContext);
                        var userName = await profile.UserName(id);

                        Usr_tbUser = await NodeContext.Usr_tbUsers.Where(u => u.UserName == userName).SingleOrDefaultAsync();

                        if (Usr_tbUser == null)
                            return NotFound();
                                        
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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Profile profile = new(NodeContext);

                Usr_tbUser.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));
                Usr_tbUser.UpdatedOn = DateTime.Now;

                NodeContext.Attach(Usr_tbUser).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Usr_tbUsers.AnyAsync(e => e.UserId == Usr_tbUser.UserId))
                        return NotFound();
                    else
                        throw;

                }

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
