using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class AddCashCodeModel : DI_BasePageModel
    {
        public AddCashCodeModel(NodeContext context) : base(context) { }

        [BindProperty]
        public string ParentKey { get; set; }

        [BindProperty]
        public string Code { get; set; }

        public bool OperationSucceeded { get; private set; }
        public string ErrorMessage { get; private set; }

        // Not bound; rebuilt each request
        public List<SelectListItem> CodeList { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(string parentKey, bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentKey))
                {
                    ErrorMessage = "Missing parent key.";
                    await PopulateCodesAsync(null);
                    return embed ? Content("<div class='text-danger small p-2'>Missing parent key</div>") : Page();
                }

                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == parentKey)
                    .Select(c => new { c.CategoryCode, c.IsEnabled })
                    .FirstOrDefaultAsync();

                if (parent == null || parent.IsEnabled == 0)
                {
                    ErrorMessage = "Parent not found or disabled.";
                    await PopulateCodesAsync(null, parentKey);
                    return embed ? Content("<div class='text-danger small p-2'>Parent not found or disabled</div>") : Page();
                }

                ParentKey = parentKey;
                // Clear any stale model state for Code so the TagHelper uses the property value
                ModelState.Remove(nameof(Code));

                await PopulateCodesAsync(Code, parentKey);
                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                ModelState.Remove(nameof(Code));
                await PopulateCodesAsync(Code, parentKey);
                return embed ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(bool embed = false)
        {
            var isEmbedded =
                embed
                || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);

            Code = Code?.Trim();

            try
            {
                if (string.IsNullOrWhiteSpace(ParentKey))
                {
                    ErrorMessage = "Missing parent key.";
                    ModelState.Remove(nameof(Code));
                    await PopulateCodesAsync(Code, ParentKey);
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing parent key</div>") : Page();
                }

                if (string.IsNullOrWhiteSpace(Code))
                {
                    ErrorMessage = "Select a Code to attach.";
                    ModelState.Remove(nameof(Code));
                    await PopulateCodesAsync(null, ParentKey);
                    return Page();
                }

                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == ParentKey)
                    .Select(c => new { c.CategoryCode, c.IsEnabled })
                    .FirstOrDefaultAsync();

                if (parent == null || parent.IsEnabled == 0)
                {
                    ErrorMessage = "Parent not found or disabled.";
                    ModelState.Remove(nameof(Code));
                    await PopulateCodesAsync(Code, ParentKey);
                    return Page();
                }

                var codeRow = await NodeContext.Cash_tbCodes
                    .Where(cd => cd.CashCode == Code)
                    .Select(cd => new {
                        cd.CashCode,
                        cd.CashDescription,
                        cd.CategoryCode,
                        cd.IsEnabled
                    })
                    .FirstOrDefaultAsync();

                if (codeRow == null || codeRow.IsEnabled == 0)
                {
                    ErrorMessage = "Code not found or disabled.";
                    ModelState.Remove(nameof(Code));
                    await PopulateCodesAsync(Code, ParentKey);
                    return Page();
                }

                if (string.Equals(codeRow.CategoryCode, ParentKey, StringComparison.OrdinalIgnoreCase))
                {
                    OperationSucceeded = true;
                    // Success marker path
                    return Page();
                }

                var rowToUpdate = await NodeContext.Cash_tbCodes
                    .Where(cd => cd.CashCode == Code)
                    .FirstOrDefaultAsync();

                if (rowToUpdate == null)
                {
                    ErrorMessage = "Code not found.";
                    ModelState.Remove(nameof(Code));
                    await PopulateCodesAsync(Code, ParentKey);
                    return Page();
                }

                rowToUpdate.CategoryCode = ParentKey;
                await NodeContext.SaveChangesAsync();

                OperationSucceeded = true;

                if (isEmbedded)
                {
                    return Page();
                }

                return RedirectToPage("./Index", new { key = "code:" + Code });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                ModelState.Remove(nameof(Code));
                await PopulateCodesAsync(Code, ParentKey);
                return isEmbedded ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }

        private async Task PopulateCodesAsync(string selectedCode = null, string parentKey = null)
        {
            var query =
                from cd in NodeContext.Cash_tbCodes
                join c in NodeContext.Cash_tbCategories on cd.CategoryCode equals c.CategoryCode
                where cd.IsEnabled != 0
                select new {
                    cd.CashCode,
                    cd.CashDescription,
                    cd.CategoryCode,
                    CategoryName = c.Category
                };

            var all = await query.ToListAsync();

            var filtered = all
                .Where(x => !string.Equals(x.CategoryCode, parentKey, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.CashCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!string.IsNullOrWhiteSpace(selectedCode))
            {
                // Ensure model property reflects intended selection
                Code = selectedCode;
                // Remove any stale model state so TagHelper uses Code property
                ModelState.Remove(nameof(Code));
            }

            CodeList = filtered
                .Select(x => new SelectListItem {
                    Value = x.CashCode,
                    Text = $"{x.CashCode} - {x.CashDescription} (from {x.CategoryName})"
                })
                .ToList();
        }
    }
}
