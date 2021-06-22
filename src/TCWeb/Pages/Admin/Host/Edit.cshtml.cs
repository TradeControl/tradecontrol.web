using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Host
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context) 
        {
            UserManager = userManager;
        }

        [BindProperty]
        public App_tbHost App_tbHost { get; set; }

        public async Task<IActionResult> OnGetAsync(int? hostId)
        {
            try
            {
                if (hostId == null)
                    return NotFound();


                App_tbHost = await NodeContext.App_tbHosts.Where(h => h.HostId == hostId).FirstOrDefaultAsync();

                if (App_tbHost == null)
                    return NotFound();
                else
                {
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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Profile profile = new(NodeContext);
                App_tbHost.InsertedBy = await profile.UserName(UserManager.GetUserId(User));
                App_tbHost.InsertedOn = DateTime.Now;

                NodeContext.Attach(App_tbHost).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.App_tbHosts.AnyAsync(e => e.HostId == App_tbHost.HostId))
                        return base.NotFound();
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

