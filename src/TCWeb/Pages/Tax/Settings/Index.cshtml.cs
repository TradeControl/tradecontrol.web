using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Settings
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Cash_vwTaxType> App_TaxTypes { get; set; }

        [BindProperty]
        [Display(Name = "Net Profit Category")]
        public string NetProfitCode { get; set; }

        [BindProperty]
        [Display(Name = "Vat Categories")]
        public string VatCategoryCode { get; set; }

        public SelectList Categories { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                App_TaxTypes = await NodeContext.App_TaxTypes.OrderBy(t => t.TaxTypeCode).ToListAsync();

                var validRootNames = await GetValidRootNamesAsync();
                Categories = new SelectList(validRootNames);

                var options = await NodeContext.App_tbOptions.FirstAsync();

                NetProfitCode = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == options.NetProfitCode)
                    .Select(c => c.Category)
                    .FirstOrDefaultAsync();

                VatCategoryCode = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == options.VatCategoryCode)
                    .Select(c => c.Category)
                    .FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                // Map posted names -> codes
                var netProfitCode = await NodeContext.Cash_tbCategories
                    .Where(t => t.Category == NetProfitCode)
                    .Select(t => t.CategoryCode)
                    .FirstOrDefaultAsync();
                var vatCode = await NodeContext.Cash_tbCategories
                    .Where(t => t.Category == VatCategoryCode)
                    .Select(t => t.CategoryCode)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(netProfitCode) || string.IsNullOrEmpty(vatCode))
                {
                    ModelState.AddModelError(string.Empty, "Please select valid categories.");
                    await ReloadListsAsync();
                    return Page();
                }

                // Server-side enforcement to match DB trigger: enabled, true root, has children
                if (!await IsTrueRootWithChildrenAsync(netProfitCode))
                {
                    ModelState.AddModelError(nameof(NetProfitCode), "Net Profit root must be an enabled root category with children.");
                    await ReloadListsAsync();
                    return Page();
                }
                if (!await IsTrueRootWithChildrenAsync(vatCode))
                {
                    ModelState.AddModelError(nameof(VatCategoryCode), "VAT root must be an enabled root category with children.");
                    await ReloadListsAsync();
                    return Page();
                }

                var options = await NodeContext.App_tbOptions.FirstAsync();
                options.NetProfitCode = netProfitCode;
                options.VatCategoryCode = vatCode;

                NodeContext.Attach(options).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.App_tbOptions.AnyAsync())
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task ReloadListsAsync()
        {
            await SetViewData();
            App_TaxTypes = await NodeContext.App_TaxTypes.OrderBy(t => t.TaxTypeCode).ToListAsync();
            Categories = new SelectList(await GetValidRootNamesAsync());
        }

        private async Task<bool> IsTrueRootWithChildrenAsync(string categoryCode)
        {
            var enabled = await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == categoryCode && c.IsEnabled != 0);
            if (!enabled) return false;

            var hasParent = await NodeContext.Cash_tbCategoryTotals.AnyAsync(t => t.ChildCode == categoryCode);
            if (hasParent) return false;

            var hasChildren = await NodeContext.Cash_tbCategoryTotals.AnyAsync(t => t.ParentCode == categoryCode);
            return hasChildren;
        }

        private async Task<IList<string>> GetValidRootNamesAsync()
        {
            var childCodes = await NodeContext.Cash_tbCategoryTotals
                .Select(t => t.ChildCode)
                .Distinct()
                .ToListAsync();

            var parentCodes = await NodeContext.Cash_tbCategoryTotals
                .Select(t => t.ParentCode)
                .Distinct()
                .ToListAsync();

            return await NodeContext.Cash_tbCategories
                .Where(c => c.IsEnabled != 0
                            && !childCodes.Contains(c.CategoryCode)
                            && parentCodes.Contains(c.CategoryCode))
                .OrderBy(c => c.Category)
                .Select(c => c.Category)
                .ToListAsync();
        }
    }
}

