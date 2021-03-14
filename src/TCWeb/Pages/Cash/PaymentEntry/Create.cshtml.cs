using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class CreateModel : PageModel
    {
        private readonly TradeControl.Web.Data.TCNodeContext _context;

        public CreateModel(TradeControl.Web.Data.TCNodeContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_vwPaymentsUnposted { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.VwPaymentsUnposteds.Add(Cash_vwPaymentsUnposted);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
