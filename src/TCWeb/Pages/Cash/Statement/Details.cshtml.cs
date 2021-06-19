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

namespace TradeControl.Web.Pages.Cash.Statement
{
    public class DetailsModel : DI_BasePageModel
    {
        [BindProperty]
        public Cash_tbPayment Cash_Payment { get; set; }

        public DetailsModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string paymentCode)
        {
            if (paymentCode == null)
                return NotFound();

            Cash_Payment = await NodeContext.Cash_tbPayments.FirstOrDefaultAsync(m => m.PaymentCode == paymentCode);

            if (Cash_Payment == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}
