using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class DetailsModel : PageModel
    {
        private readonly TradeControl.Web.Data.NodeContext _context;

        public DetailsModel(TradeControl.Web.Data.NodeContext context)
        {
            _context = context;
        }

        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Cash_PaymentsUnposted = await _context.Cash_PaymentsUnposted.FirstOrDefaultAsync(m => m.PaymentCode == id);

            if (Cash_PaymentsUnposted == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
