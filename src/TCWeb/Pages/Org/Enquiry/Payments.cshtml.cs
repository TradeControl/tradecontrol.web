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

namespace TradeControl.Web.Pages.Org.Enquiry
{
    public class PaymentsModel : DI_BasePageModel
    {
        [BindProperty]
        public IList<Cash_vwPayment> Org_Payments { get; set; }

        [BindProperty]
        public Org_vwAccountLookup Org_Account { get; set; }

        public PaymentsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (string.IsNullOrEmpty(accountCode))
                return NotFound();

            Org_Account = await NodeContext.Org_AccountLookup.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

            if (Org_Account == null)
                return NotFound();

            var payments = from tb in NodeContext.Cash_Payments
                           where tb.AccountCode == accountCode
                           orderby tb.PaidOn descending
                           select tb;

            Org_Payments = await payments.ToListAsync();

            await SetViewData();
            return Page();


        }
    }
}
