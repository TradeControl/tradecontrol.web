using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;


namespace TradeControl.Web.Pages.Org.Update
{
    [Authorize(Roles = "Administrators, Managers")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_tbOrg Org_tbOrg { get; set; }

        [BindProperty]
        public string OrganisationType { get; set; }

        public SelectList OrganisationTypes { get; set; }

        [BindProperty]
        public string OrganisationStatus { get; set; }
        public SelectList OrganisationStatuses { get; set; }

        [BindProperty]
        public string TaxDescription { get; set; }
        public SelectList TaxCodes { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            try
            {
                Org_tbOrg = await NodeContext.Org_tbOrgs.FindAsync(accountCode);

                if (Org_tbOrg == null)
                    return NotFound();
                else
                {
                    OrganisationTypes = new SelectList(await NodeContext.Org_tbTypes.OrderBy(t => t.OrganisationTypeCode).Select(t => t.OrganisationType).ToListAsync());
                    OrganisationStatuses = new SelectList(await NodeContext.Org_tbStatuses.OrderBy(t => t.OrganisationStatusCode).Select(t => t.OrganisationStatus).ToListAsync());
                    TaxCodes = new SelectList(await NodeContext.App_TaxCodes.OrderBy(t => t.TaxCode).Select(t => t.TaxDescription).ToListAsync());

                    TaxDescription = await NodeContext.App_TaxCodes.Where(t => t.TaxCode == Org_tbOrg.TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();
                    OrganisationType = await NodeContext.Org_tbTypes.Where(t => t.OrganisationTypeCode == Org_tbOrg.OrganisationTypeCode).Select(t => t.OrganisationType).FirstAsync();
                    OrganisationStatus = await NodeContext.Org_tbStatuses.Where(t => t.OrganisationStatusCode == Org_tbOrg.OrganisationStatusCode).Select(t => t.OrganisationStatus).FirstAsync();

                    await SetViewData();
                    return Page();
                }
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }

        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Profile profile = new(NodeContext);
                Org_tbOrg.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

                Org_tbOrg.OrganisationStatusCode = await NodeContext.Org_tbStatuses.Where(t => t.OrganisationStatus == OrganisationStatus).Select(t => t.OrganisationStatusCode).FirstAsync();
                Org_tbOrg.OrganisationTypeCode = await NodeContext.Org_tbTypes.Where(t => t.OrganisationType == OrganisationType).Select(t => t.OrganisationTypeCode).FirstAsync();
                Org_tbOrg.TaxCode = await NodeContext.App_tbTaxCodes.Where(t => t.TaxDescription == TaxDescription).Select(t => t.TaxCode).FirstAsync();

                NodeContext.Attach(Org_tbOrg).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Org_tbOrgs.AnyAsync(e => e.AccountCode == Org_tbOrg.AccountCode))
                        return NotFound();
                    else
                        throw;

                }

                RouteValueDictionary route = new();
                route.Add("accountCode", Org_tbOrg.AccountCode);
                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
