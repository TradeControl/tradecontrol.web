using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace TradeControl.Web.Pages.Org.Type
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        public Org_vwTypeLookup Org_Type { get; set; }

        [BindProperty]
        [Display(Name ="Accounts")]
        public int NumberOfAccounts { get; set; }

        public async Task<IActionResult> OnGetAsync(short? organisationTypeCode)
        {
            try
            {
                if (organisationTypeCode == null)
                    return NotFound();

                Org_Type = await NodeContext.Org_TypeLookup.FirstOrDefaultAsync(t => t.OrganisationTypeCode == organisationTypeCode);

                if (Org_Type == null)
                    return NotFound();

                NumberOfAccounts = await NodeContext.Org_tbOrgs.Where(o => o.OrganisationTypeCode == organisationTypeCode).CountAsync();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(short? organisationTypeCode)
        {
            try
            {
                if (organisationTypeCode == null)
                    return NotFound();

                var tbOrgType = await NodeContext.Org_tbTypes.FindAsync(organisationTypeCode);
                NodeContext.Org_tbTypes.Remove(tbOrgType);
                await NodeContext.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
