using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    /// <summary>
    /// CategoryTree maintenance endpoints and helpers (state-changing logic only).
    /// </summary>
    [Authorize]
    public partial class CategoryTreeModel : DI_BasePageModel
    {
        #region Display Order
        /// <summary>
        /// Returns true if the working set's DisplayOrder is initialised (no zero values).
        /// Evaluates the correct set based on <paramref name="parentKey"/> (root, disconnected, or specific parent).
        /// </summary>
        private async Task<bool> IsDisplayOrderInitialised(string parentKey)
        {
            if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var orders = await NodeContext.Cash_tbCategories
                    .Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
                    .Select(c => c.DisplayOrder)
                    .ToListAsync();

                return orders.Count == 0 || orders.All(v => v > 0);
            }

            if (IsRootKey(parentKey))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var rootOrders = await NodeContext.Cash_tbCategories
                    .Where(c => !childCodes.Contains(c.CategoryCode) && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
                    .Select(c => c.DisplayOrder)
                    .ToListAsync();

                return rootOrders.Count == 0 || rootOrders.All(v => v > 0);
            }
            else
            {
                var siblingOrders = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == parentKey)
                    .Select(t => t.DisplayOrder)
                    .ToListAsync();

                return siblingOrders.Count == 0 || siblingOrders.All(v => v > 0);
            }
        }

        /// <summary>
        /// Initialises DisplayOrder for the working set matched by <paramref name="parentKey"/>.
        /// Root/disconnected use Cash_tbCategory.DisplayOrder; child sets use Cash_tbCategoryTotal.DisplayOrder.
        /// Fallback ordering groups by CashPolarityCode then by Category.
        /// </summary>
        private async Task DisplayOrderInitialise(string parentKey)
        {
            // Disconnected set initialisation
            if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var list = await NodeContext.Cash_tbCategories
                    .Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
                    .OrderBy(c => c.CashPolarityCode)
                    .ThenBy(c => c.Category)
                    .ToListAsync();

                short i = 1;
                foreach (var c in list)
                    c.DisplayOrder = i++;

                await NodeContext.SaveChangesAsync();
                return;
            }

            // Root set initialisation
            if (IsRootKey(parentKey))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var roots = await NodeContext.Cash_tbCategories
                    .Where(c => !childCodes.Contains(c.CategoryCode) && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
                    .OrderBy(c => c.CashPolarityCode)
                    .ThenBy(c => c.Category)
                    .ToListAsync();

                short i = 1;
                foreach (var c in roots)
                    c.DisplayOrder = i++;

                await NodeContext.SaveChangesAsync();
            }
            else
            {
                // Child set initialisation
                var totalsForParent = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == parentKey)
                    .ToListAsync();

                var childCodes = totalsForParent.Select(t => t.ChildCode).ToArray();
                var childCats = await NodeContext.Cash_tbCategories
                    .Where(c => childCodes.Contains(c.CategoryCode))
                    .ToDictionaryAsync(c => c.CategoryCode);

                var orderedTotals = totalsForParent
                    .OrderBy(t => childCats[t.ChildCode].CashPolarityCode)
                    .ThenBy(t => childCats[t.ChildCode].Category)
                    .ToList();

                short i = 1;
                foreach (var t in orderedTotals)
                    t.DisplayOrder = i++;

                await NodeContext.SaveChangesAsync();
            }
        }
        #endregion

        #region enablement
        /// <summary>
        /// Enable/Disable a node.
        /// - If key refers to a code (key starts with "code:"), toggle only that code.
        /// - If key refers to a category, toggle that category and cascade to all descendant categories only.
        /// Never updates Cash_tbCodes for category cascades.
        /// </summary>
        public async Task<JsonResult> OnPostSetEnabledAsync([FromForm] string key, [FromForm] short enabled)
        {
            try
            {
                if (!User.IsInRole(Constants.AdministratorsRole))
                    return new JsonResult(new { success = false, message = "Insufficient privileges" });

                if (string.IsNullOrWhiteSpace(key))
                    return new JsonResult(new { success = false, message = "Missing key" });

                // UI: enabled 1=enable, 0=disable. DB: IsEnabled 1=enabled, 0=disabled.
                short newState = enabled != 0 ? (short)1 : (short)0;

                if (key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
                {
                    // Toggle a single code only
                    var cashCode = key.Substring("code:".Length);
                    var affected = await NodeContext.Cash_tbCodes
                        .Where(c => c.CashCode == cashCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsEnabled, newState));

                    return new JsonResult(new { success = affected > 0, isEnabled = newState, nodeType = "code" });
                }

                // Toggle a category and strictly cascade to descendant categories only (no codes)
                var exists = await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == key);
                if (!exists)
                    return new JsonResult(new { success = false, message = "Category not found" });

                var edges = await NodeContext.Cash_tbCategoryTotals
                                .Select(t => new { t.ParentCode, t.ChildCode })
                                .ToListAsync();

                var stack = new Stack<string>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                stack.Push(key);
                while (stack.Count > 0)
                {
                    var cur = stack.Pop();
                    if (!seen.Add(cur))
                        continue;

                    foreach (var ch in edges.Where(e => e.ParentCode == cur && !string.IsNullOrEmpty(e.ChildCode)).Select(e => e.ChildCode))
                        stack.Push(ch);
                }

                var affectedCats = await NodeContext.Cash_tbCategories
                    .Where(c => seen.Contains(c.CategoryCode))
                    .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsEnabled, newState));

                return new JsonResult(new { success = affectedCats > 0, isEnabled = newState, nodeType = "category", affectedCategories = affectedCats });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = e.Message });
            }
        }
        #endregion

        private static JsonResult NotImplemented(string action, string key, string parentKey, string nodeType = null) =>
            new(new { success = false, message = $"Not Yet Implemented: {action}", key, parentKey, nodeType });

        // View is allowed for all authenticated users (useful for mobile)
        public Task<JsonResult> OnPostViewAsync([FromForm] string key, [FromForm] string parentKey)
            => Task.FromResult(NotImplemented("View", key, parentKey, key != null && key.StartsWith("code:", StringComparison.OrdinalIgnoreCase) ? "code" : "category"));

        // Admin-only below
        private bool IsAdmin() => User.IsInRole(Constants.AdministratorsRole);

        /// <summary>
        /// Create a new Total category (and optionally attach it under a parent).
        /// Accepts either:
        ///  - minimal form: parentKey only -> returns helpful message (client should open full create page), or
        ///  - full creation fields: categoryCode + category + cashTypeCode + cashPolarityCode (will create category and attach)
        /// Returns JSON: { success, message, key, parentKey }
        /// </summary>
        public async Task<JsonResult> OnPostCreateTotalAsync(
            [FromForm] string parentKey,
            [FromForm] string categoryCode,
            [FromForm] string category,
            [FromForm] short? cashTypeCode,
            [FromForm] short? cashPolarityCode,
            [FromForm] short? isEnabled)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            // If required creation fields not provided, return a hint so client can open full Create page
            if (string.IsNullOrWhiteSpace(categoryCode) || string.IsNullOrWhiteSpace(category))
            {
                return new JsonResult(new {
                    success = false,
                    message = "Missing creation data. Open the CreateTotal action/page to provide details."
                });
            }

            try
            {
                // Ensure category code is unique
                if (await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == categoryCode))
                    return new JsonResult(new { success = false, message = "Category code already exists." });

                var now = DateTime.UtcNow;
                var user = User?.Identity?.Name ?? "system";

                var cat = new Cash_tbCategory {
                    CategoryCode = categoryCode,
                    Category = category,
                    CategoryTypeCode = (short)NodeEnum.CategoryType.CashTotal,
                    CashTypeCode = cashTypeCode ?? 0,
                    CashPolarityCode = cashPolarityCode ?? (short)NodeEnum.CashPolarity.Neutral,
                    DisplayOrder = 0,
                    IsEnabled = (isEnabled.HasValue ? (short)(isEnabled.Value != 0 ? 1 : 0) : (short)1),
                    InsertedBy = user,
                    InsertedOn = now,
                    UpdatedBy = user,
                    UpdatedOn = now
                };

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                NodeContext.Cash_tbCategories.Add(cat);
                await NodeContext.SaveChangesAsync();

                // If a parentKey was provided, attach under it with next display order
                if (!string.IsNullOrWhiteSpace(parentKey))
                {
                    short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey)
                        .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                    NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal {
                        ParentCode = parentKey,
                        ChildCode = categoryCode,
                        DisplayOrder = nextOrder
                    });

                    await NodeContext.SaveChangesAsync();
                }

                await tx.CommitAsync();

                return new JsonResult(new { success = true, message = "Category created", key = categoryCode, parentKey = parentKey ?? "" });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }

        /// <summary>
        /// Create a new Cash Code under a category.
        /// Expects categoryCode, taxCode (or fallback), cashCode, cashDescription.
        /// Returns JSON: { success, message, cashCode }
        /// </summary>
        public async Task<JsonResult> OnPostCreateCodeByCategoryAsync(
            [FromForm] string categoryCode,
            [FromForm] string taxCode,
            [FromForm] string cashCode,
            [FromForm] string cashDescription,
            [FromForm] string templateCode = null)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(categoryCode) || string.IsNullOrWhiteSpace(cashCode) || string.IsNullOrWhiteSpace(cashDescription))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                // Validate category exists and enabled
                var catExists = await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == categoryCode && c.IsEnabled != 0);
                if (!catExists)
                    return new JsonResult(new { success = false, message = "Category not found or disabled." });

                // Ensure cash code uniqueness
                if (await NodeContext.Cash_tbCodes.AnyAsync(c => c.CashCode == cashCode))
                    return new JsonResult(new { success = false, message = "Cash code already exists." });

                // Resolve tax code: if provided must exist; otherwise pick any or fail
                string finalTaxCode = taxCode;
                if (string.IsNullOrWhiteSpace(finalTaxCode))
                {
                    var anyTax = await NodeContext.App_tbTaxCodes.FirstOrDefaultAsync();
                    if (anyTax == null)
                        return new JsonResult(new { success = false, message = "No tax codes available; supply a valid taxCode." });
                    finalTaxCode = anyTax.TaxCode;
                }
                else
                {
                    var taxExists = await NodeContext.App_tbTaxCodes.AnyAsync(t => t.TaxCode == finalTaxCode);
                    if (!taxExists)
                        return new JsonResult(new { success = false, message = "Tax code not found." });
                }

                var now = DateTime.UtcNow;
                var user = User?.Identity?.Name ?? "system";

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                var code = new Cash_tbCode {
                    CashCode = cashCode,
                    CashDescription = cashDescription,
                    CategoryCode = categoryCode,
                    TaxCode = finalTaxCode,
                    IsEnabled = 1,
                    InsertedBy = user,
                    InsertedOn = now,
                    UpdatedBy = user,
                    UpdatedOn = now
                };

                NodeContext.Cash_tbCodes.Add(code);
                await NodeContext.SaveChangesAsync();
                await tx.CommitAsync();

                return new JsonResult(new { success = true, message = "Cash code created", cashCode = cashCode });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }

        /// <summary>
        /// Add an existing category as a child under a specified parent (totals).
        /// Ensures parent is CashTotal, prevents cycles and duplicate mappings.
        /// </summary>
        public async Task<JsonResult> OnPostAddExistingTotalAsync([FromForm] string parentKey, [FromForm] string childKey)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(parentKey) || string.IsNullOrWhiteSpace(childKey))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            if (string.Equals(parentKey, childKey, StringComparison.OrdinalIgnoreCase))
                return new JsonResult(new { success = false, message = "Parent and child cannot be the same." });

            try
            {
                // Validate parent exists, enabled and is a Total
                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == parentKey && c.IsEnabled != 0)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode })
                    .FirstOrDefaultAsync();

                if (parent == null)
                    return new JsonResult(new { success = false, message = "Parent category not found or disabled." });

                if (parent.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                    return new JsonResult(new { success = false, message = "Parent must be a Total-type category." });

                // Validate child exists and enabled
                var child = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == childKey && c.IsEnabled != 0)
                    .Select(c => c.CategoryCode)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(child))
                    return new JsonResult(new { success = false, message = "Child category not found or disabled." });

                // Prevent duplicate mapping
                var exists = await NodeContext.Cash_tbCategoryTotals.AnyAsync(t => t.ParentCode == parentKey && t.ChildCode == childKey);
                if (exists)
                    return new JsonResult(new { success = false, message = "Child already attached to parent." });

                // Prevent cycles: ensure parent is not a descendant of child
                var parentMap = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode != null && t.ParentCode != null)
                    .GroupBy(t => t.ChildCode)
                    .Select(g => new { Child = g.Key, Parent = g.Select(x => x.ParentCode).FirstOrDefault() })
                    .ToDictionaryAsync(x => x.Child, x => x.Parent);

                var cur = parentKey;
                var guard = 0;
                while (!string.IsNullOrEmpty(cur) && guard++ < 1024)
                {
                    if (string.Equals(cur, childKey, StringComparison.OrdinalIgnoreCase))
                        return new JsonResult(new { success = false, message = "Operation would create a cycle." });

                    if (!parentMap.TryGetValue(cur, out var p) || string.IsNullOrEmpty(p))
                        break;

                    cur = p;
                }

                // Insert mapping
                short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == parentKey)
                    .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal {
                    ParentCode = parentKey,
                    ChildCode = childKey,
                    DisplayOrder = nextOrder
                });

                await NodeContext.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Child added", key = childKey, parentKey });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }

        /// <summary>
        /// Add an existing Cash Code to a category (reassign code to the target category).
        /// </summary>
        public async Task<JsonResult> OnPostAddExistingCodeAsync([FromForm] string parentKey, [FromForm] string codeKey)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(parentKey) || string.IsNullOrWhiteSpace(codeKey))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                var category = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == parentKey && c.IsEnabled != 0)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode })
                    .FirstOrDefaultAsync();

                if (category == null)
                    return new JsonResult(new { success = false, message = "Target category not found or disabled." });

                // Optionally enforce category type (only allow attaching to CashCode categories).
                // This follows the UI rule that codes belong under CategoryType == CashCode (0).
                if (category.CategoryTypeCode != (short)NodeEnum.CategoryType.CashCode)
                    return new JsonResult(new { success = false, message = "Target category is not a Cash Code category." });

                var code = await NodeContext.Cash_tbCodes.FirstOrDefaultAsync(c => c.CashCode == codeKey);
                if (code == null)
                    return new JsonResult(new { success = false, message = "Cash code not found." });

                // Reassign the code
                code.CategoryCode = parentKey;
                NodeContext.Attach(code).State = EntityState.Modified;
                await NodeContext.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Cash code reassigned", cashCode = codeKey, parentKey });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }

        /// <summary>
        /// CreateCode shortcut - if only parentKey provided instruct client to open full create page.
        /// </summary>
        public Task<JsonResult> OnPostCreateCodeAsync([FromForm] string parentKey)
        {
            if (!IsAdmin())
                return Task.FromResult(new JsonResult(new { success = false, message = "Insufficient privileges" }));

            if (string.IsNullOrWhiteSpace(parentKey))
                return Task.FromResult(new JsonResult(new { success = false, message = "Missing parentKey. Open the Create Code page." }));

            return Task.FromResult(new JsonResult(new { success = false, message = "Open the CreateCode page to provide details.", parentKey }));
        }

        /// <summary>
        /// Edit invoked from context menu - instruct client to open full Edit page.
        /// </summary>
        public Task<JsonResult> OnPostEditAsync([FromForm] string key)
        {
            if (!IsAdmin())
                return Task.FromResult(new JsonResult(new { success = false, message = "Insufficient privileges" }));

            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult(new JsonResult(new { success = false, message = "Missing key" }));

            return Task.FromResult(new JsonResult(new { success = false, message = "Open the Edit page for the selected node.", key }));
        }


        public async Task<JsonResult> OnPostDeleteCashCodeAsync([FromForm] string key)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(key) || !key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
                return new JsonResult(new { success = false, message = "Missing or invalid key" });

            try
            {
                var cashCode = key.Substring("code:".Length);
                var code = await NodeContext.Cash_tbCodes.FirstOrDefaultAsync(c => c.CashCode == cashCode);
                if (code == null)
                    return new JsonResult(new { success = false, message = "Cash code not found." });

                // Quick safety check: refuse delete if payments reference this cash code
                var paymentCount = await NodeContext.Cash_tbPayments.Where(p => p.CashCode == cashCode).CountAsync();
                if (paymentCount > 0)
                    return new JsonResult(new { success = false, message = "Cannot delete Cash Code: related payments exist.", payments = paymentCount });

                NodeContext.Cash_tbCodes.Remove(code);
                await NodeContext.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Cash code deleted.", cashCode = cashCode });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }

        public async Task<JsonResult> OnPostDeleteCategoryAsync([FromForm] string key)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(key))
                return new JsonResult(new { success = false, message = "Missing key" });

            try
            {
                // Verify category exists
                var cat = await NodeContext.Cash_tbCategories.FirstOrDefaultAsync(c => c.CategoryCode == key);
                if (cat == null)
                    return new JsonResult(new { success = false, message = "Category not found." });

                // Build descendant set (recursive)
                var edges = await NodeContext.Cash_tbCategoryTotals
                                .Select(t => new { t.ParentCode, t.ChildCode })
                                .ToListAsync();

                var stack = new Stack<string>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                stack.Push(key);
                while (stack.Count > 0)
                {
                    var cur = stack.Pop();
                    if (!seen.Add(cur)) continue;
                    foreach (var ch in edges.Where(e => e.ParentCode == cur && !string.IsNullOrEmpty(e.ChildCode)).Select(e => e.ChildCode))
                        stack.Push(ch);
                }

                // If any cash codes exist in the delete set, refuse — caller must remove or reassign codes first.
                var codeCount = await NodeContext.Cash_tbCodes.Where(c => seen.Contains(c.CategoryCode)).CountAsync();
                if (codeCount > 0)
                {
                    return new JsonResult(new { success = false, message = "Cannot delete category tree while Cash Codes exist. Remove or reassign codes first.", codes = codeCount });
                }

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                // Remove totals mappings where parent or child in set
                await NodeContext.Cash_tbCategoryTotals
                    .Where(t => seen.Contains(t.ParentCode) || seen.Contains(t.ChildCode))
                    .ExecuteDeleteAsync();

                // Remove category rows
                await NodeContext.Cash_tbCategories
                    .Where(c => seen.Contains(c.CategoryCode))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return new JsonResult(new { success = true, message = "Category deleted", key, deletedCount = seen.Count });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }

        public async Task<JsonResult> OnPostDeleteTotalAsync([FromForm] string parentKey, [FromForm] string childKey)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(parentKey) || string.IsNullOrWhiteSpace(childKey))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                // Ensure mapping exists (defensive)
                var mappingExists = await NodeContext.Cash_tbCategoryTotals
                    .AnyAsync(t => t.ParentCode == parentKey && t.ChildCode == childKey);

                if (!mappingExists)
                    return new JsonResult(new { success = false, message = "Mapping not found." });

                // Build descendant set from childKey
                var edges = await NodeContext.Cash_tbCategoryTotals
                                .Select(t => new { t.ParentCode, t.ChildCode })
                                .ToListAsync();

                var stack = new Stack<string>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                stack.Push(childKey);

                while (stack.Count > 0)
                {
                    var cur = stack.Pop();
                    if (!seen.Add(cur))
                        continue;

                    foreach (var ch in edges
                        .Where(e => string.Equals(e.ParentCode, cur, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(e.ChildCode))
                        .Select(e => e.ChildCode))
                    {
                        stack.Push(ch);
                    }
                }

                // Partition seen into totals and non-totals
                var cats = await NodeContext.Cash_tbCategories
                    .Where(c => seen.Contains(c.CategoryCode))
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode })
                    .ToListAsync();

                short totalType = (short)NodeEnum.CategoryType.CashTotal;
                var totalsToDelete = new HashSet<string>(cats
                    .Where(c => c.CategoryTypeCode == totalType)
                    .Select(c => c.CategoryCode), StringComparer.OrdinalIgnoreCase);

                if (totalsToDelete.Count == 0)
                {
                    return new JsonResult(new { success = true, message = "No total categories to delete.", parentKey, childKey, deletedTotals = 0 });
                }

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                // Remove any totals mappings involving nodes in totalsToDelete
                await NodeContext.Cash_tbCategoryTotals
                    .Where(t => totalsToDelete.Contains(t.ParentCode) || totalsToDelete.Contains(t.ChildCode))
                    .ExecuteDeleteAsync();

                // Defensive: ensure direct mapping is also removed
                await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == parentKey && t.ChildCode == childKey)
                    .ExecuteDeleteAsync();

                // Delete the total categories themselves
                var deletedTotals = await NodeContext.Cash_tbCategories
                    .Where(c => totalsToDelete.Contains(c.CategoryCode))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return new JsonResult(new {
                    success = true,
                    message = "Total category subtree deleted.",
                    parentKey,
                    childKey,
                    deletedTotals
                });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }

        /// <summary>
        /// Create a category (used for disconnected creation or explicit category creation).
        /// Accepts parentKey, categoryCode, category, cashTypeCode, cashPolarityCode, isEnabled.
        /// If parentKey == DisconnectedNodeKey no totals mapping is created.
        /// Returns JSON: { success, message, key, parentKey }.
        /// </summary>
        public async Task<JsonResult> OnPostCreateCategoryAsync(
            [FromForm] string parentKey,
            [FromForm] string categoryCode,
            [FromForm] string category,
            [FromForm] short? cashTypeCode,
            [FromForm] short? cashPolarityCode,
            [FromForm] short? isEnabled)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(categoryCode) || string.IsNullOrWhiteSpace(category))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                if (await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == categoryCode))
                    return new JsonResult(new { success = false, message = "Category code already exists." });

                var cat = new Cash_tbCategory {
                    CategoryCode = categoryCode,
                    Category = category,
                    CategoryTypeCode = (short)NodeEnum.CategoryType.CashTotal,
                    CashTypeCode = cashTypeCode ?? (short)NodeEnum.CashType.Trade,
                    CashPolarityCode = cashPolarityCode ?? (short)NodeEnum.CashPolarity.Neutral,
                    DisplayOrder = 0,
                    IsEnabled = (isEnabled.HasValue ? (short)(isEnabled.Value != 0 ? 1 : 0) : (short)1)
                };

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                NodeContext.Cash_tbCategories.Add(cat);
                await NodeContext.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(parentKey) && !string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey)
                        .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                    NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal {
                        ParentCode = parentKey,
                        ChildCode = categoryCode,
                        DisplayOrder = nextOrder
                    });

                    await NodeContext.SaveChangesAsync();
                }

                await tx.CommitAsync();

                return new JsonResult(new { success = true, message = "Category created", key = categoryCode, parentKey = parentKey ?? "" });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }
    }
}
