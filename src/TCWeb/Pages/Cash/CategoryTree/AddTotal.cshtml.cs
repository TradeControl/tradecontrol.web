using System;
using System.Collections.Generic;
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
    public class AddTotalModel : DI_BasePageModel
    {
        public AddTotalModel(NodeContext context) : base(context) { }

        [BindProperty]
        public string ParentKey { get; set; }

        [BindProperty]
        public string ChildKey { get; set; }

        public string ChildName { get; private set; }
        public short ChildPolarity { get; private set; }
        public bool ChildIsEnabled { get; private set; }

        public bool OperationSucceeded { get; private set; }
        public string ErrorMessage { get; private set; }

        public List<SelectListItem> TotalOptions { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(string parentKey, bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentKey))
                {
                    ErrorMessage = "Missing parent key.";
                    await PopulateOptionsAsync(null);
                    return embed ? Content("<div class='text-danger small p-2'>Missing parent key</div>") : Page();
                }

                // Validate parent exists and is Total-type + enabled
                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == parentKey)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode, c.IsEnabled })
                    .FirstOrDefaultAsync();

                if (parent == null || parent.IsEnabled == 0)
                {
                    ErrorMessage = "Parent not found or disabled.";
                    await PopulateOptionsAsync(null, parentKey);
                    return embed ? Content("<div class='text-danger small p-2'>Parent not found or disabled</div>") : Page();
                }
                if (parent.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Parent must be a Total-type category.";
                    await PopulateOptionsAsync(null, parentKey);
                    return embed ? Content("<div class='text-danger small p-2'>Invalid parent type</div>") : Page();
                }

                ParentKey = parentKey;
                await PopulateOptionsAsync(null, parentKey);
                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                await PopulateOptionsAsync(null, parentKey);
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
                if (string.IsNullOrWhiteSpace(ParentKey))
                {
                    ErrorMessage = "Missing parent key.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing parent key</div>") : Page();
                }

                if (string.IsNullOrWhiteSpace(ChildKey))
                {
                    ErrorMessage = "Select a Total to attach.";
                    await PopulateOptionsAsync(null, ParentKey);
                    return Page();
                }

                ChildKey = ChildKey.Trim();
                if (string.Equals(ParentKey, ChildKey, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Parent and child cannot be the same.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                // Parent validation
                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == ParentKey)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode, c.IsEnabled })
                    .FirstOrDefaultAsync();
                if (parent == null || parent.IsEnabled == 0)
                {
                    ErrorMessage = "Parent not found or disabled.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }
                if (parent.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Parent must be a Total-type category.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                // Child validation
                var child = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == ChildKey)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode, c.Category, c.CashPolarityCode, c.IsEnabled })
                    .FirstOrDefaultAsync();
                if (child == null || child.IsEnabled == 0)
                {
                    ErrorMessage = "Child not found or disabled.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }
                if (child.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Child must be a Total-type category.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                // Already attached to this parent?
                var exists = await NodeContext.Cash_tbCategoryTotals
                    .AnyAsync(t => t.ParentCode == ParentKey && t.ChildCode == ChildKey);
                if (exists)
                {
                    ErrorMessage = "Child already attached to this parent.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                // Already attached anywhere? (enforce single-parent for totals)
                var alreadyParented = await NodeContext.Cash_tbCategoryTotals
                    .AnyAsync(t => t.ChildCode == ChildKey);

                if (alreadyParented)
                {
                    ErrorMessage = "Child already attached to another parent.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                // Cycle prevention (walk up from parent)
                var parentMap = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode != null && t.ParentCode != null)
                    .GroupBy(t => t.ChildCode)
                    .Select(g => new { Child = g.Key, Parent = g.Select(x => x.ParentCode).FirstOrDefault() })
                    .ToDictionaryAsync(x => x.Child, x => x.Parent);

                var cur = ParentKey;
                var guard = 0;
                while (!string.IsNullOrEmpty(cur) && guard++ < 1024)
                {
                    if (string.Equals(cur, ChildKey, StringComparison.OrdinalIgnoreCase))
                    {
                        ErrorMessage = "Operation would create a cycle.";
                        await PopulateOptionsAsync(ChildKey, ParentKey);
                        return Page();
                    }
                    if (!parentMap.TryGetValue(cur, out var p) || string.IsNullOrEmpty(p))
                        break;
                    cur = p;
                }

                // Normalise DisplayOrder if needed
                var siblingOrders = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == ParentKey)
                    .Select(t => t.DisplayOrder)
                    .ToListAsync();
                if (siblingOrders.Count > 0 && siblingOrders.Any(o => o == 0))
                {
                    short i = 1;
                    var orderedExisting = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == ParentKey)
                        .OrderBy(t => t.DisplayOrder)
                        .ToListAsync();
                    foreach (var row in orderedExisting)
                        row.DisplayOrder = i++;
                    await NodeContext.SaveChangesAsync();
                }

                short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == ParentKey)
                    .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal {
                    ParentCode = ParentKey,
                    ChildCode = ChildKey,
                    DisplayOrder = nextOrder
                });
                await NodeContext.SaveChangesAsync();

                ChildName = child.Category;
                ChildPolarity = child.CashPolarityCode;
                ChildIsEnabled = child.IsEnabled != 0;
                OperationSucceeded = true;

                if (isEmbedded)
                    return Page(); // marker for client JS

                return RedirectToPage("./Index", new { key = ChildKey });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                await PopulateOptionsAsync(ChildKey, ParentKey);
                return isEmbedded ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }

        private async Task PopulateOptionsAsync(string selectedChildCode = null, string parentKey = null)
        {
            // Child codes already attached to this parent (exclude)
            var attached = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(parentKey))
            {
                var attachedList = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == parentKey)
                    .Select(t => t.ChildCode)
                    .ToListAsync();
                attached = new HashSet<string>(attachedList, StringComparer.OrdinalIgnoreCase);
            }

            short totalType = (short)NodeEnum.CategoryType.CashTotal;

            var totals = await NodeContext.Cash_tbCategories
                .Where(c => c.CategoryTypeCode == totalType && c.IsEnabled != 0)
                .OrderBy(c => c.CashPolarityCode)
                .ThenBy(c => c.Category)
                .Select(c => new { c.CategoryCode, c.Category, c.CashPolarityCode })
                .ToListAsync();

            TotalOptions = totals
                .Where(c => !string.Equals(c.CategoryCode, parentKey, StringComparison.OrdinalIgnoreCase)) // not self
                .Where(c => !attached.Contains(c.CategoryCode)) // not already attached to this parent
                .Select(c => new SelectListItem {
                    Value = c.CategoryCode,
                    Text = $"{c.Category} ({c.CategoryCode})",
                    Selected = !string.IsNullOrWhiteSpace(selectedChildCode)
                               && string.Equals(selectedChildCode, c.CategoryCode, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }
    }
}
