using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Update
{
    [Authorize(Roles = "Administrators, Managers")]
    public class DeleteModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_vwDatasheet OrgAccount { get; set; }

        public DeleteModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (accountCode == null)
                return NotFound();

            OrgAccount = await NodeContext.Subject_Datasheet.FirstOrDefaultAsync(m => m.SubjectCode == accountCode);

            if (OrgAccount == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAsync(string accountCode)
        {
            try
            {
                if (accountCode == null)
                    return NotFound();

                OrgAccount = await NodeContext.Subject_Datasheet.FindAsync(accountCode);

                if (OrgAccount != null)
                {
                    var tbSubject = await NodeContext.Subject_tbSubjects.FindAsync(accountCode);
                    NodeContext.Subject_tbSubjects.Remove(tbSubject);
                    await NodeContext.SaveChangesAsync();
                }

                RouteValueDictionary route = new();
                route.Add("SubjectType", OrgAccount.SubjectType);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
