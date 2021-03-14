using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Calendar
{
    public class DeleteModel : PageModel
    {
        private readonly TradeControl.Web.Data.TCNodeContext _context;

        public DeleteModel(TradeControl.Web.Data.TCNodeContext context)
        {
            _context = context;
        }

        [BindProperty]
        public App_tbCalendar App_tbCalendar { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            App_tbCalendar = await _context.App_tbCalendars.FirstOrDefaultAsync(m => m.CalendarCode == id);

            if (App_tbCalendar == null)
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

            App_tbCalendar = await _context.App_tbCalendars.FindAsync(id);

            if (App_tbCalendar != null)
            {
                _context.App_tbCalendars.Remove(App_tbCalendar);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
