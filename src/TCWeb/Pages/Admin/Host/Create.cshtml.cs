using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Host
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        [BindProperty]
        public App_tbHost App_tbHost { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context) 
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await SetViewData();


                Profile profile = new(NodeContext);
                var userName = await profile.UserName(UserManager.GetUserId(User));

                App_tbHost = new()
                {
                    InsertedBy = userName,
                    InsertedOn = DateTime.Now
                };

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

                NodeContext.App_tbHosts.Add(App_tbHost);
                await NodeContext.SaveChangesAsync();
                
                if (await NodeContext.App_tbHosts.AnyAsync())
                {
                    int hostId = await NodeContext.App_tbHosts.Select(h => h.HostId).FirstAsync();
                    NodeSettings nodeSettings = new(NodeContext);
                    if (!await nodeSettings.SetHost(hostId))
                        return RedirectToPage("/Admin/EventLog/Index");
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