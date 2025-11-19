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
    public class AddCategoryModel : DI_BasePageModel
    {
        public AddCategoryModel(NodeContext context) : base(context) { }

        [BindProperty]
        public string ParentKey { get; set; }

        [BindProperty]
        public string ChildKey { get; set; }

        public string ChildName { get; private set; }
        public short ChildPolarity { get; private set; }
        public bool ChildIsEnabled { get; private set; }

        public bool OperationSucceeded { get; private set; }
        public string ErrorMessage { get; private set; }

        [BindProperty]
        public List<SelectListItem> CategoryList { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(string parentKey, bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentKey))
                {
                    ErrorMessage = "Missing parent key.";
                    await PopulateOptionsAsync(null);
                    return embed
                        ? Content("<div class='text-danger small p-2'>Missing parent key</div>")
                        : Page();
                }

                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == parentKey)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode, c.IsEnabled })
                    .FirstOrDefaultAsync();

                if (parent == null || parent.IsEnabled == 0)
                {
                    ErrorMessage = "Parent not found or disabled.";
                    await PopulateOptionsAsync(null, parentKey);
                    return embed
                        ? Content("<div class='text-danger small p-2'>Parent not found or disabled</div>")
                        : Page();
                }

                if (parent.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Parent category type is invalid for this operation.";
                    await PopulateOptionsAsync(null, parentKey);
                    return embed
                        ? Content("<div class='text-danger small p-2'>Invalid parent type</div>")
                        : Page();
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
                return embed
                    ? Content("<div class='text-danger small p-2'>Server error</div>")
                    : Page();
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
                    return isEmbedded
                        ? Content("<div class='text-danger small p-2'>Missing parent key</div>")
                        : Page();
                }

                if (string.IsNullOrWhiteSpace(ChildKey))
                {
                    ErrorMessage = "Select a Category to attach.";
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
                    ErrorMessage = "Parent category type is invalid for this operation.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                var child = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == ChildKey)
                    .Select(c => new {
                        c.CategoryCode,
                        c.CategoryTypeCode,
                        c.Category,
                        c.CashPolarityCode,
                        c.IsEnabled
                    })
                    .FirstOrDefaultAsync();

                if (child == null || child.IsEnabled == 0)
                {
                    ErrorMessage = "Child not found or disabled.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                if (child.CategoryTypeCode == (short)NodeEnum.CategoryType.Expression)
                {
                    ErrorMessage = "Child category type is invalid for this operation.";
                    await PopulateOptionsAsync(ChildKey, ParentKey);
                    return Page();
                }

                // Current attachment
                var currentLink = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == ChildKey)
                    .Select(t => new { t.ParentCode })
                    .FirstOrDefaultAsync();

                // Idempotent success
                if (currentLink != null &&
                    string.Equals(currentLink.ParentCode, ParentKey, StringComparison.OrdinalIgnoreCase))
                {
                    ChildName = child.Category;
                    ChildPolarity = child.CashPolarityCode;
                    ChildIsEnabled = child.IsEnabled != 0;
                    OperationSucceeded = true;

                    if (isEmbedded)
                        return Page();

                    return RedirectToPage("/Cash/CategoryTree/Index",
                        new { select = ChildKey, parentKey = ParentKey, expand = ParentKey });
                }

                // Build ancestor map for cycle detection (ParentKey upwards)
                var parentMap = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode != null && t.ParentCode != null)
                    .GroupBy(t => t.ChildCode)
                    .Select(g => new {
                        Child = g.Key,
                        Parent = g.Select(x => x.ParentCode).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.Child, x => x.Parent);

                bool cycleDetected = IsAncestor(ChildKey, ParentKey, parentMap);

                if (cycleDetected)
                {
                    ChildName = child.Category;
                    ChildPolarity = child.CashPolarityCode;
                    ChildIsEnabled = child.IsEnabled != 0;
                    OperationSucceeded = true;

                    if (isEmbedded)
                        return Page();

                    return RedirectToPage("/Cash/CategoryTree/Index",
                        new { select = ChildKey, parentKey = ParentKey, expand = ParentKey });
                }

                // MOVE: detach from old parent if different
                if (currentLink != null &&
                    !string.Equals(currentLink.ParentCode, ParentKey, StringComparison.OrdinalIgnoreCase))
                {
                    using (var tx = await NodeContext.Database.BeginTransactionAsync())
                    {
                        var oldLink = await NodeContext.Cash_tbCategoryTotals
                            .Where(t => t.ParentCode == currentLink.ParentCode && t.ChildCode == ChildKey)
                            .FirstOrDefaultAsync();

                        if (oldLink != null)
                        {
                            NodeContext.Cash_tbCategoryTotals.Remove(oldLink);
                            await NodeContext.SaveChangesAsync();
                        }

                        var oldSiblings = await NodeContext.Cash_tbCategoryTotals
                            .Where(t => t.ParentCode == currentLink.ParentCode)
                            .OrderBy(t => t.DisplayOrder)
                            .ToListAsync();

                        short reIdx = 1;
                        foreach (var s in oldSiblings)
                        {
                            s.DisplayOrder = reIdx++;
                        }
                        await NodeContext.SaveChangesAsync();

                        await tx.CommitAsync();
                    }
                }

                // Normalize display order under new parent
                var siblingOrders = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == ParentKey)
                    .Select(t => t.DisplayOrder)
                    .ToListAsync();

                if (siblingOrders.Count > 0 && siblingOrders.Any(o => o == 0))
                {
                    var orderedExisting = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == ParentKey)
                        .OrderBy(t => t.DisplayOrder)
                        .ToListAsync();

                    short i = 1;
                    foreach (var row in orderedExisting)
                    {
                        row.DisplayOrder = i++;
                    }
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
                    return Page();

                return RedirectToPage("/Cash/CategoryTree/Index",
                    new { select = ChildKey, parentKey = ParentKey, expand = ParentKey });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                await PopulateOptionsAsync(ChildKey, ParentKey);
                return isEmbedded
                    ? Content("<div class='text-danger small p-2'>Server error</div>")
                    : Page();
            }
        }

        private async Task PopulateOptionsAsync(string selectedChildCode = null, string parentKey = null)
        {
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
            short expressionType = (short)NodeEnum.CategoryType.Expression;

            var rows = await (from c in NodeContext.Cash_tbCategories
                              join ct in NodeContext.Cash_tbTypes on c.CashTypeCode equals ct.CashTypeCode
                              where c.IsEnabled != 0
                                 && c.CategoryTypeCode != expressionType
                              select new {
                                  c.CategoryCode,
                                  c.Category,
                                  c.CashPolarityCode,
                                  c.CategoryTypeCode,
                                  CashType = ct.CashType
                              }).ToListAsync();

            rows = rows
                .Where(r => !string.Equals(r.CategoryCode, parentKey, StringComparison.OrdinalIgnoreCase))
                .Where(r => !attached.Contains(r.CategoryCode))
                .ToList();

            var ordered = rows
                .OrderByDescending(r => r.CategoryTypeCode == totalType)
                .ThenBy(r => r.CashPolarityCode)
                .ThenBy(r => r.Category, StringComparer.OrdinalIgnoreCase)
                .ToList();

            CategoryList = ordered
                .Select(r => {
                    var text = $"{r.Category} ({r.CategoryCode})";
                    return new SelectListItem {
                        Value = r.CategoryCode,
                        Text = text,
                        Selected = !string.IsNullOrWhiteSpace(selectedChildCode)
                                   && string.Equals(selectedChildCode, r.CategoryCode, StringComparison.OrdinalIgnoreCase)
                    };
                })
                .ToList();
        }

        private static bool IsAncestor(string possibleAncestor, string startChild, IDictionary<string, string> parentMap)
        {
            var cur = startChild;
            var guard = 0;
            while (!string.IsNullOrEmpty(cur) && guard++ < 1024)
            {
                if (string.Equals(cur, possibleAncestor, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!parentMap.TryGetValue(cur, out var p) || string.IsNullOrEmpty(p))
                    break;

                cur = p;
            }
            return false;
        }
    }
}
