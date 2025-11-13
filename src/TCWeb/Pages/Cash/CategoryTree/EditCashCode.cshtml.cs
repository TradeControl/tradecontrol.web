using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditCashCodeModel : DI_BasePageModel
    {
        public EditCashCodeModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string Key { get; set; } = "";

        [BindProperty]
        public string CashCode { get; set; } = "";

        [BindProperty]
        public string CashDescription { get; set; } = "";

        // Dropdown binding (select by description, store code)
        [BindProperty]
        public string TaxDescription { get; set; } = "";

        [BindProperty]
        public string TaxCode { get; set; } = "";

        [BindProperty]
        public bool IsEnabled { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public string ParentKey { get; set; } = "";

        public short ParentCashType { get; private set; } = 0;

        public bool OperationSucceeded { get; private set; } = false;
        public string ErrorMessage { get; private set; } = "";

        public SelectList TaxDescriptions { get; private set; }

        public async Task<IActionResult> OnGetAsync(string key, string parentKey = "", bool embed = false)
        {
            try
            {
                var raw = (key ?? "").Trim();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    ErrorMessage = "Missing key.";
                    await PopulateTaxDescriptionsAsync(null);
                    return embed ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                CashCode = raw;
                var row = await NodeContext.Cash_tbCodes
                    .Where(c => c.CashCode == raw)
                    .Select(c => new { c.CashCode, c.CashDescription, c.TaxCode, c.IsEnabled, c.CategoryCode })
                    .SingleOrDefaultAsync();

                if (row == null)
                {
                    ErrorMessage = "Cash code not found.";
                    await PopulateTaxDescriptionsAsync(null);
                    return embed ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                CashDescription = row.CashDescription;
                TaxCode = row.TaxCode;
                IsEnabled = row.IsEnabled != 0;

                // Resolve the description for current TaxCode and build dropdown
                TaxDescription = await NodeContext.App_tbTaxCodes
                    .Where(t => t.TaxCode == TaxCode)
                    .Select(t => t.TaxDescription)
                    .FirstOrDefaultAsync() ?? "";

                await PopulateTaxDescriptionsAsync(TaxDescription);

                ParentKey = string.IsNullOrWhiteSpace(parentKey) ? row.CategoryCode : parentKey;

                try
                {
                    ParentCashType = await NodeContext.Cash_tbCategories
                        .Where(c => c.CategoryCode == ParentKey)
                        .Select(c => c.CashTypeCode)
                        .FirstOrDefaultAsync();
                }
                catch
                {
                    ParentCashType = 0;
                }

                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                await PopulateTaxDescriptionsAsync(null);
                return embed ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(bool embed = false)
        {
            var isEmbedded =
                embed
                || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);

            try
            {
                var raw = (Key ?? CashCode ?? "").Trim();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    ErrorMessage = "Missing key.";
                    await PopulateTaxDescriptionsAsync(TaxDescription);
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var row = await NodeContext.Cash_tbCodes.FirstOrDefaultAsync(c => c.CashCode == raw);
                if (row == null)
                {
                    ErrorMessage = "Cash code not found.";
                    await PopulateTaxDescriptionsAsync(TaxDescription);
                    return Page();
                }

                // Validate TaxDescription -> map to TaxCode
                if (string.IsNullOrWhiteSpace(TaxDescription))
                {
                    ModelState.AddModelError(nameof(TaxDescription), "Select a tax code.");
                    await PopulateTaxDescriptionsAsync(null);
                    return Page();
                }

                var selectedTaxCode = await NodeContext.App_tbTaxCodes
                    .Where(t => t.TaxDescription == TaxDescription)
                    .Select(t => t.TaxCode)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(selectedTaxCode))
                {
                    ModelState.AddModelError(nameof(TaxDescription), "Invalid tax code selection.");
                    await PopulateTaxDescriptionsAsync(TaxDescription);
                    return Page();
                }

                // Persist edits
                row.CashDescription = (CashDescription ?? "").Trim();
                row.TaxCode = selectedTaxCode;
                row.IsEnabled = IsEnabled ? (short)1 : (short)0;

                await NodeContext.SaveChangesAsync();

                // Load parent cash type for marker icon
                try
                {
                    ParentCashType = await NodeContext.Cash_tbCategories
                        .Where(c => c.CategoryCode == row.CategoryCode)
                        .Select(c => c.CashTypeCode)
                        .FirstOrDefaultAsync();
                }
                catch
                {
                    ParentCashType = 0;
                }

                CashCode = row.CashCode;
                OperationSucceeded = true;

                if (isEmbedded)
                {
                    return Page(); // emits editCashCodeResult marker
                }

                return RedirectToPage("/Cash/CategoryTree/Index", new { key = "code:" + CashCode });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                await PopulateTaxDescriptionsAsync(TaxDescription);
                return Page();
            }
        }

        private async Task PopulateTaxDescriptionsAsync(string selectedDescription)
        {
            var taxes = await NodeContext.App_tbTaxCodes
                .OrderBy(t => t.TaxDescription)
                .Select(t => new { t.TaxDescription })
                .ToListAsync();

            TaxDescriptions = new SelectList(
                taxes,
                nameof(App_tbTaxCode.TaxDescription),
                nameof(App_tbTaxCode.TaxDescription),
                selectedDescription
            );
        }
    }
}
