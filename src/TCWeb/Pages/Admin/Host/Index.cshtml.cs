using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Host
{
    [Authorize(Roles = "Administrators")]
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<App_tbHost> App_Hosts { get; set; }

        public SelectList HostDescriptions { get; set; }

        [BindProperty]
        [Display(Name = "Active Host")]
        public string HostDescription { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                App_Hosts = await NodeContext.App_tbHosts.OrderBy(h => h.HostDescription).ToListAsync();
                HostDescriptions = new SelectList(await NodeContext.App_tbHosts.OrderBy(h => h.HostDescription).Select(h => h.HostDescription).ToListAsync());
                HostDescription = await NodeContext.App_Host.Select(h => h.HostDescription).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                int hostId = await NodeContext.App_tbHosts.Where(h => h.HostDescription == HostDescription).Select(h => h.HostId).FirstAsync();

                NodeSettings settings = new(NodeContext);

                if (await settings.SetHost(hostId))
                    return RedirectToPage("./Index");
                else
                    return RedirectToPage("/Admin/EventLog/Index");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
