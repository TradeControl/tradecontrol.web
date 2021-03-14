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
    public class IndexModel : PageModel
    {
        private readonly TradeControl.Web.Data.TCNodeContext _context;

        public IndexModel(TradeControl.Web.Data.TCNodeContext context)
        {
            _context = context;
        }

        public IList<App_tbCalendar> App_tbCalendar { get;set; }

        public async Task OnGetAsync()
        {
            App_tbCalendar = await _context.App_tbCalendars.ToListAsync();
        }
    }
}
