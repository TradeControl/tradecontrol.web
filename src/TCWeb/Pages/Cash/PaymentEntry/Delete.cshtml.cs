﻿using System;
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
        private readonly TradeControl.Web.Data.TCNodeContext _context;

        public DeleteModel(TradeControl.Web.Data.TCNodeContext context)
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

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Cash_vwPaymentsUnposted = await _context.VwPaymentsUnposteds.FindAsync(id);

            if (Cash_vwPaymentsUnposted != null)
            {
                _context.VwPaymentsUnposteds.Remove(Cash_vwPaymentsUnposted);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
