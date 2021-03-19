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
    public class DetailsModel : PageModel
    {
        private readonly TradeControl.Web.Data.NodeContext _context;

        public DetailsModel(TradeControl.Web.Data.NodeContext context)
        {
            _context = context;
        }

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
    }
}
