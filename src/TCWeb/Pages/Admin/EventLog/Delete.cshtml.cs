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
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.EventLog
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

        [BindProperty]
        public App_vwEventLog App_EventLog { get; set; }


        public async Task<IActionResult> OnGetAsync(string logCode)
        {
            if (logCode == null)
                return NotFound();

            App_EventLog = await NodeContext.App_EventLogs.FirstOrDefaultAsync(e => e.LogCode == logCode);

            if (App_EventLog == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string logCode)
        {
            try
            {
                if (logCode == null)
                    return NotFound();

                NodeAdmin nodeAdmin = new(NodeContext);

                if (await nodeAdmin.EventLogCleardown(logCode))
                    return RedirectToPage("./Index");
                else
                    throw new Exception($"Log cleardown to {logCode} failed!");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
