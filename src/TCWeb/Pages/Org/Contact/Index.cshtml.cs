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

namespace TradeControl.Web.Pages.Org.Contact
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

        public IList<Org_vwContact> Org_Contacts { get; set; }


        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (string.IsNullOrEmpty(accountCode))
                return NotFound();

            Org_Account = await NodeContext.Org_tbOrgs.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

            if (Org_Account == null)
                return NotFound();

            Org_Contacts = await NodeContext.Org_Contacts
                                    .Where(c => c.AccountCode == accountCode)
                                    .OrderBy(t => t.ContactName)
                                    .ToListAsync();

            await SetViewData();
            return Page();

        }
    }
}
