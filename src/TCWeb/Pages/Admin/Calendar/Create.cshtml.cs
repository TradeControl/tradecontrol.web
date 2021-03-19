﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Calendar
{
    public class CreateModel : PageModel
    {
        private readonly TradeControl.Web.Data.NodeContext _context;

        public CreateModel(TradeControl.Web.Data.NodeContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public App_tbCalendar App_tbCalendar { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.App_tbCalendars.Add(App_tbCalendar);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
