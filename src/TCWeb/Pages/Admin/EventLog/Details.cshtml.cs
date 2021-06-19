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
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

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
    }
}
