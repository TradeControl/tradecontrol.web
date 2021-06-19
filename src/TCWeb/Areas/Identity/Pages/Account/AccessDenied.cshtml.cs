using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Pages;

namespace TradeControl.Web.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : DI_BasePageModel
    {
        public AccessDeniedModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync()
        {
            await SetViewData();
        }
    }
}

