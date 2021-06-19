using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Org.Type
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Org_vwTypeLookup> Org_Types { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Org_Types = await NodeContext.Org_TypeLookup.OrderBy(t => t.OrganisationType).ToListAsync();

            await SetViewData();
            return Page();

        }


    }
}

