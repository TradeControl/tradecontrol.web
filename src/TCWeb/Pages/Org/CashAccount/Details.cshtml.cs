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

namespace TradeControl.Web.Pages.Org.CashAccount
{
    public class DetailsModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_vwCashAccount Org_CashAccount { get; set; }

        public DetailsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string cashAccountCode)
        {
            if (cashAccountCode == null)
                return NotFound();

            Org_CashAccount = await NodeContext.Org_CashAccounts.FirstOrDefaultAsync(m => m.CashAccountCode == cashAccountCode);

            if (Org_CashAccount == null)
                return NotFound();
            else
            {

                await SetViewData();
                return Page();
            }

        }
    }
}
