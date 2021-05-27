using System;
using System.Collections.Generic;
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

namespace TradeControl.Web.Pages.Org.Address
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public Org_tbOrg Org_Account { get; set; }

        public IList<Org_vwAddressList> Org_AddressList { get; set; }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (string.IsNullOrEmpty(accountCode))
                return NotFound();

            Org_Account = await NodeContext.Org_tbOrgs.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

            if (Org_Account == null)
                return NotFound();

            Org_AddressList = await NodeContext.Org_AddressList.Where(t => t.AccountCode == accountCode).ToListAsync();

            await SetViewData();
            return Page();
        }
    }
}
