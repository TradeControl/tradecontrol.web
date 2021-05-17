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

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

        public async Task<IActionResult> OnGetAsync(string paymentCode)
        {
            if (paymentCode == null)
                return NotFound();

            Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.FirstOrDefaultAsync(m => m.PaymentCode == paymentCode);

            if (Cash_PaymentsUnposted == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}
