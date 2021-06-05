using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.EventLog
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<App_vwEventLog> App_EventLog { get; set; }

        public async Task OnGetAsync()
        {
            await SetViewData();
            var events = from tb in NodeContext.App_EventLogs
                         orderby tb.LoggedOn descending
                         select tb;

            App_EventLog = await events.ToListAsync();
        }
    }
}
