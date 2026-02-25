using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.TaxCode
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public App_tbTaxCode App_tbTaxCode { get; set; }

        [BindProperty]
        public string Rounding { get; set; }

        [BindProperty]
        public string TaxType { get; set; }

        [BindProperty]
        [Display(Name = "Tax Rate (%)")]
        public decimal TaxRatePercent { get; set; }

        public SelectList Roundings { get; set; }
        public SelectList TaxTypes { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string taxCode, string taxType, string searchString)
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
                Rounding = await NodeContext.App_tbRoundings
                    .Where(r => r.RoundingCode == App_tbTaxCode.RoundingCode)
                    .Select(r => r.Rounding)
                    .FirstAsync();

                var taxtypes = NodeContext.App_TaxCodeTypes.OrderBy(t => t.TaxTypeCode).Select(t => t.TaxType);
                TaxTypes = new SelectList(await taxtypes.ToListAsync());
                TaxType = await NodeContext.App_TaxCodeTypes
                    .Where(t => t.TaxTypeCode == App_tbTaxCode.TaxTypeCode)
                    .Select(t => t.TaxType)
                    .FirstAsync();

                TaxRatePercent = App_tbTaxCode.TaxRate * 100m;

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string embedded, string returnNode, string taxType, string searchString)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError(string.Empty, "Please correct the highlighted fields and try again.");

                    var roundings = NodeContext.App_tbRoundings.OrderBy(r => r.RoundingCode).Select(r => r.Rounding);
                    Roundings = new SelectList(await roundings.ToListAsync());

                    var taxtypes = NodeContext.App_TaxCodeTypes.OrderBy(t => t.TaxTypeCode).Select(t => t.TaxType);
                    TaxTypes = new SelectList(await taxtypes.ToListAsync());

                    await SetViewData();
                    return Page();
                }

                var embeddedMode = string.Equals(embedded, "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(embedded, "true", StringComparison.OrdinalIgnoreCase);

                returnNode = string.IsNullOrWhiteSpace(returnNode) ? "TaxCode" : returnNode;

                // TaxCode is immutable here: do not trim/overwrite/regenerate it.
                App_tbTaxCode.TaxDescription = (App_tbTaxCode.TaxDescription ?? string.Empty).Trim();

                App_tbTaxCode.RoundingCode = await NodeContext.App_tbRoundings
                    .Where(r => r.Rounding == Rounding)
                    .Select(r => r.RoundingCode)
                    .FirstAsync();

                App_tbTaxCode.TaxTypeCode = await NodeContext.App_TaxCodeTypes
                    .Where(t => t.TaxType == TaxType)
                    .Select(t => t.TaxTypeCode)
                    .FirstAsync();

                App_tbTaxCode.TaxRate = TaxRatePercent / 100m;

                var profile = new Profile(NodeContext);
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
                    throw;
                }

                return RedirectToPage("./Index", new {
                    embedded = embeddedMode ? "1" : null,
                    returnNode,
                    taxType,
                    searchString
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
