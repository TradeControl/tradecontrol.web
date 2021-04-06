using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Calendar
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public App_tbCalendar App_tbCalendar { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
                return NotFound();

            App_tbCalendar = await NodeContext.App_tbCalendars.FirstOrDefaultAsync(m => m.CalendarCode == id);

            if (App_tbCalendar == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            App_tbCalendar = await NodeContext.App_tbCalendars.FindAsync(id);

            if (App_tbCalendar != null)
            {
                NodeContext.App_tbCalendars.Remove(App_tbCalendar);
                await NodeContext.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
