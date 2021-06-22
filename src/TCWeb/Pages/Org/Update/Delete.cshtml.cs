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

namespace TradeControl.Web.Pages.Org.Update
{
    [Authorize(Roles = "Administrators, Managers")]
    public class DeleteModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_vwDatasheet OrgAccount { get; set; }

        public DeleteModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (accountCode == null)
                return NotFound();

            OrgAccount = await NodeContext.Org_Datasheet.FirstOrDefaultAsync(m => m.AccountCode == accountCode);

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

                OrgAccount = await NodeContext.Org_Datasheet.FindAsync(accountCode);

                if (OrgAccount != null)
                {
                    var tbOrg = await NodeContext.Org_tbOrgs.FindAsync(accountCode);
                    NodeContext.Org_tbOrgs.Remove(tbOrg);
                    await NodeContext.SaveChangesAsync();
                }

                RouteValueDictionary route = new();
                route.Add("OrganisationType", OrgAccount.OrganisationType);

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
