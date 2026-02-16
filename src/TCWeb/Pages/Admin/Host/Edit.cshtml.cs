using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;
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

        [BindProperty(SupportsGet = true)]
        public string? ReturnNode { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Embedded { get; set; }

        public async Task<IActionResult> OnGetAsync(int? hostId)
        {
            try
            {
                if (hostId == null)
                    return NotFound();

                App_tbHost = await NodeContext.App_tbHosts.Where(h => h.HostId == hostId).FirstOrDefaultAsync();

                if (App_tbHost == null)
                    return NotFound();

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
                App_tbHost.InsertedBy = await profile.UserName(UserManager.GetUserId(User));
                App_tbHost.InsertedOn = DateTime.Now;

                NodeSettings nodeSettings = new(NodeContext);
                var (key, iv) = await nodeSettings.GetOrCreateSymmetricAsync();

                Encrypt encrypt = new(key, iv);

                if (App_tbHost.IsSmtpAuth)
                    App_tbHost.EmailPassword = encrypt.EncryptString(App_tbHost.EmailPassword);
                else
                    App_tbHost.EmailPassword = string.Empty;

                NodeContext.Attach(App_tbHost).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.App_tbHosts.AnyAsync(e => e.HostId == App_tbHost.HostId))
                        return base.NotFound();

                    throw;
                }

                if (Embedded == 1)
                    return Redirect($"/Admin/Host/Index?embedded=1&returnNode={Uri.EscapeDataString(ReturnNode ?? "Host")}");

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
