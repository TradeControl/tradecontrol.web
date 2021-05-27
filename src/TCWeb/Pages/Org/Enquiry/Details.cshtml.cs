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

namespace TradeControl.Web.Pages.Org.Enquiry
{
    public class DetailsModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_vwDatasheet Org_Account { get; set; }

        [BindProperty]
        public IList<Org_tbAddress> Org_Addresses { get; set; }

        [BindProperty]
        public IList<Org_tbContact> Org_Contacts { get; set; }

        public DetailsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (accountCode == null)
                return NotFound();

            Org_Account = await NodeContext.Org_Datasheet.FirstOrDefaultAsync(m => m.AccountCode == accountCode);

            if (Org_Account == null)
                return NotFound();
            else
            {
                Org_Contacts = await NodeContext.Org_tbContacts.Where(t => t.AccountCode == accountCode).OrderBy(t => t.ContactName).ToListAsync();
                Org_Addresses = await NodeContext.Org_tbAddresses.Where(t => t.AccountCode == accountCode && t.AddressCode != Org_Account.AddressCode).ToListAsync();

                await SetViewData();
                return Page();
            }

        }
    }
}
