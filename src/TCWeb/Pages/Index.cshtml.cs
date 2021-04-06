using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages
{
    [AllowAnonymous]
    public class IndexModel : DI_BasePageModel
    {

        public IndexModel(
            ILogger<IndexModel> logger,
            NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager) : base(logger, context, authorizationService, userManager)
        {
        }

        public async Task OnGetAsync()
        {
            await SetViewData();
        }
    }
}
