using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Calendar
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(NodeContext context) : base(context) {}

        public async Task<IActionResult> OnGetAsync()
        {
            await SetViewData();
            return Page();
        }

        [BindProperty]
        public App_tbCalendar App_tbCalendar { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            NodeContext.App_tbCalendars.Add(App_tbCalendar);
            await NodeContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
