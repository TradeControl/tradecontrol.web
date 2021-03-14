using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class EditModel : PageModel
    {
        private readonly TradeControl.Web.Data.TCNodeContext _context;

        public EditModel(TradeControl.Web.Data.TCNodeContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_vwPaymentsUnposted { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Cash_vwPaymentsUnposted = await _context.VwPaymentsUnposteds.FirstOrDefaultAsync(m => m.PaymentCode == id);

            if (Cash_vwPaymentsUnposted == null)
            {
                return NotFound();
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            _context.Attach(Cash_vwPaymentsUnposted).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Cash_vwPaymentsUnpostedExists(Cash_vwPaymentsUnposted.PaymentCode))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool Cash_vwPaymentsUnpostedExists(string id)
        {
            return _context.VwPaymentsUnposteds.Any(e => e.PaymentCode == id);
        }
    }
}
