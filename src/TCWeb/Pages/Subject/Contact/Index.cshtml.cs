using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Contact
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Subject_tbSubject Subject_Account { get; set; }

        public IList<Subject_vwContact> Subject_Contacts { get; set; }


        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (string.IsNullOrEmpty(accountCode))
                return NotFound();

            Subject_Account = await NodeContext.Subject_tbSubjects.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

            if (Subject_Account == null)
                return NotFound();

            Subject_Contacts = await NodeContext.Subject_Contacts
                                    .Where(c => c.AccountCode == accountCode)
                                    .OrderBy(t => t.ContactName)
                                    .ToListAsync();

            await SetViewData();
            return Page();

        }
    }
}
