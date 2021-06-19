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
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

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
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(int? hostId)
        {
            try
            {
                if (hostId == null)
                    return NotFound();

                var setting = await NodeContext.App_Host.OrderBy(h => h.HostId).SingleOrDefaultAsync();

                if (setting != null)
                {
                    if (setting.HostId == hostId)
                    {
                        NodeSettings nodeSettings = new(NodeContext);
                        if (await NodeContext.App_tbHosts.AnyAsync())
                            await nodeSettings.SetHost(await NodeContext.App_tbHosts.OrderByDescending(h => h.HostId).Select(h => h.HostId).FirstAsync());
                        else
                            await nodeSettings.SetHost(null);
                    }
                }

                var tbHost = await NodeContext.App_tbHosts.FindAsync(hostId);
                NodeContext.App_tbHosts.Remove(tbHost);
                await NodeContext.SaveChangesAsync();

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