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
    public class DeleteModel : PageModel
    {
        private readonly TradeControl.Web.Data.NodeContext _context;

        public DeleteModel(TradeControl.Web.Data.NodeContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Cash_PaymentsUnposted = await _context.Cash_PaymentsUnposted.FindAsync(id);

            if (Cash_PaymentsUnposted != null)
            {
                _context.Cash_PaymentsUnposted.Remove(Cash_PaymentsUnposted);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
