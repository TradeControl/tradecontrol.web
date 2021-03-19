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
        private readonly TradeControl.Web.Data.NodeContext _context;

        public EditModel(TradeControl.Web.Data.NodeContext context)
        {
            _context = context;
        }

        public SelectList CashCodes { get; set; }
        public SelectList TaxCodes { get; set; }

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

            if (Cash_PaymentsUnposted.CashCode != null)
            {
                var cashCodes = from t in _context.Cash_CodeLookup
                                where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                orderby t.CashCode
                                select t.CashCode;

                CashCodes = new SelectList(await cashCodes.Distinct().ToListAsync());

                var taxCodes = from t in _context.App_TaxCodes
                               orderby t.TaxCode
                               select t.TaxCode;

                TaxCodes = new SelectList(await taxCodes.Distinct().ToListAsync());
            }

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Cash_PaymentsUnposted).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Cash_vwPaymentsUnpostedExists(Cash_PaymentsUnposted.PaymentCode))
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
            return _context.Cash_PaymentsUnposted.Any(e => e.PaymentCode == id);
        }
    }
}
