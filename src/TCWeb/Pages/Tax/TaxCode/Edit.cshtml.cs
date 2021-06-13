using System;
using System.Collections.Generic;
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

namespace TradeControl.Web.Pages.Tax.TaxCode
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        [BindProperty]
        public App_tbTaxCode App_tbTaxCode { get; set; }

        [BindProperty]
        public string Rounding { get; set; }
        [BindProperty]
        public string TaxType { get; set; }

        public SelectList Roundings { get; set; }
        public SelectList TaxTypes { get; set; }

        public async Task<IActionResult> OnGetAsync(string taxCode)
        {
            try
            {
                if (string.IsNullOrEmpty(taxCode))
                    return NotFound();

                App_tbTaxCode = await NodeContext.App_tbTaxCodes.FindAsync(taxCode);
                if (App_tbTaxCode == null)
                    return NotFound();

                var roundings = NodeContext.App_tbRoundings.OrderBy(r => r.RoundingCode).Select(r => r.Rounding);
                Roundings = new SelectList(await roundings.ToListAsync());
                Rounding = await NodeContext.App_tbRoundings.Where(r => r.RoundingCode == App_tbTaxCode.RoundingCode).Select(r => r.Rounding).FirstAsync();

                var taxtypes = NodeContext.App_TaxCodeTypes.OrderBy(t => t.TaxTypeCode).Select(t => t.TaxType);
                TaxTypes = new SelectList(await taxtypes.ToListAsync());
                TaxType = await NodeContext.App_TaxCodeTypes.Where(t => t.TaxTypeCode == App_tbTaxCode.TaxTypeCode).Select(t => t.TaxType).FirstAsync();

                await SetViewData();
                return Page();
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

                App_tbTaxCode.RoundingCode = await NodeContext.App_tbRoundings.Where(r => r.Rounding == Rounding).Select(r => r.RoundingCode).FirstAsync();
                App_tbTaxCode.TaxTypeCode = await NodeContext.App_TaxCodeTypes.Where(t => t.TaxType == TaxType).Select(t => t.TaxTypeCode).FirstAsync();

                Profile profile = new(NodeContext);
                App_tbTaxCode.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));
                App_tbTaxCode.UpdatedOn = DateTime.Now;

                NodeContext.Attach(App_tbTaxCode).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.App_tbTaxCodes.AnyAsync(e => e.TaxCode == App_tbTaxCode.TaxCode))
                        return NotFound();
                    else
                        throw;

                }

                RouteValueDictionary route = new();
                route.Add("taxCode", App_tbTaxCode.TaxCode);

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
