using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradeControl.Web.Authorization;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    /// <summary>
    /// CategoryTree maintenance endpoints and helpers (state-changing logic only).
    /// </summary>
    [Authorize]
    public partial class CategoryTreeModel
    {
        #region Display Order
        /// <summary>
        /// Returns true if the working set's DisplayOrder is initialised (no zero values).
        /// Evaluates the correct set based on <paramref name="parentKey"/> (root, disconnected, or specific parent).
        /// </summary>
        private async Task<bool> IsDisplayOrderInitialised(string versionId, string parentKey)
        {
            if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals
                    .Where(item => item.VersionId == versionId).ToListAsync();
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var orders = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && c.IsEnabled != 0
                        && !linkedSet.Contains(c.CategoryCode))
                    .Select(c => c.DisplayOrder)
                    .ToListAsync();

                return orders.Count == 0 || orders.All(v => v > 0);
            }

            if (IsRootKey(parentKey))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals
                    .Where(item => item.VersionId == versionId).ToListAsync();
                var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var rootOrders = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && !childCodes.Contains(c.CategoryCode)
                        && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
                    .Select(c => c.DisplayOrder)
                    .ToListAsync();

                return rootOrders.Count == 0 || rootOrders.All(v => v > 0);
            }
            else
            {
                var siblingOrders = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == versionId && t.ParentCode == parentKey)
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
        private async Task DisplayOrderInitialise(string versionId, string parentKey)
        {
            // Disconnected set initialisation
            if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals
                    .Where(item => item.VersionId == versionId).ToListAsync();
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var list = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && c.IsEnabled != 0
                        && !linkedSet.Contains(c.CategoryCode))
                    .OrderBy(c => c.CashPolarityCode)
                    .ThenBy(c => c.Category)
                    .ToListAsync();

                await SaveCategoryOrdersAsync(versionId,
                    list.Select((category, index) => (category.CategoryCode, (short)(index + 1))),
                    "Disconnected category ordering initialised.");
                return;
            }

            // Root set initialisation
            if (IsRootKey(parentKey))
            {
                var totals = await NodeContext.Cash_tbCategoryTotals
                    .Where(item => item.VersionId == versionId).ToListAsync();
                var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
                var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                var roots = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && !childCodes.Contains(c.CategoryCode)
                        && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
                    .OrderBy(c => c.CashPolarityCode)
                    .ThenBy(c => c.Category)
                    .ToListAsync();

                await SaveCategoryOrdersAsync(versionId,
                    roots.Select((category, index) => (category.CategoryCode, (short)(index + 1))),
                    "Root category ordering initialised.");
            }
            else
            {
                // Child set initialisation
                var totalsForParent = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == versionId && t.ParentCode == parentKey)
                    .ToListAsync();

                var childCodes = totalsForParent.Select(t => t.ChildCode).ToArray();
                var childCats = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && childCodes.Contains(c.CategoryCode))
                    .ToDictionaryAsync(c => c.CategoryCode);

                var orderedTotals = totalsForParent
                    .OrderBy(t => childCats[t.ChildCode].CashPolarityCode)
                    .ThenBy(t => childCats[t.ChildCode].Category)
                    .ToList();

                await SaveTotalOrdersAsync(versionId, parentKey,
                    orderedTotals.Select((total, index) => (total.ChildCode, (short)(index + 1))),
                    "Child category ordering initialised.");
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
        public async Task<JsonResult> OnPostSetEnabledAsync([FromForm] string versionId,
            [FromForm] string key, [FromForm] short enabled)
        {
            try
            {
                if (!User.IsInRole(Constants.AdministratorsRole))
                    return new JsonResult(new { success = false, message = "Insufficient privileges" });

                if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(key))
                    return new JsonResult(new { success = false, message = "Missing version or key" });

                // UI: enabled 1=enable, 0=disable. DB: IsEnabled 1=enabled, 0=disabled.
                short newState = enabled != 0 ? (short)1 : (short)0;

                if (key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
                {
                    // Toggle a single code only
                    var cashCode = key.Substring("code:".Length);
                    var code = await NodeContext.Cash_tbCodes.AsNoTracking()
                        .SingleOrDefaultAsync(c => c.CashCode == cashCode);
                    if (code is null)
                        return new JsonResult(new { success = false, message = "Cash Code not found" });
                    await CategoryTreeService.UpdateCashCodeAsync(new CategoryTreeCashCodeUpdateCommand(
                        versionId, cashCode, code.CashDescription, code.TaxCode, newState));
                    return new JsonResult(new { success = true, isEnabled = newState, nodeType = "code" });
                }

                // Toggle a category and strictly cascade to descendant categories only (no codes)
                var exists = await NodeContext.Cash_tbCategories
                    .AnyAsync(c => c.VersionId == versionId && c.CategoryCode == key);
                if (!exists)
                    return new JsonResult(new { success = false, message = "Category not found" });

                var edges = await NodeContext.Cash_tbCategoryTotals
                                .Where(t => t.VersionId == versionId)
                                .Select(t => new { t.ParentCode, t.ChildCode, t.DisplayOrder })
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

                var affected = await NodeContext.Cash_tbCategories.AsNoTracking()
                    .Where(c => c.VersionId == versionId && seen.Contains(c.CategoryCode))
                    .ToListAsync();
                var commands = affected.Select(c => new CategoryTreeCategoryCommand(versionId,
                    c.CategoryCode, c.Category, c.CategoryTypeCode, c.CashPolarityCode,
                    c.CashTypeCode, c.DisplayOrder, newState, "Category enablement changed.")).ToArray();
                await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(versionId,
                    commands, Array.Empty<CategoryTreeTotalCommand>(),
                    Array.Empty<CategoryTreeCashCodeMoveCommand>()));

                return new JsonResult(new { success = commands.Length > 0, isEnabled = newState, nodeType = "category", affectedCategories = commands.Length });
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

        #region Posting Helpers
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
            [FromForm] string versionId,
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
            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(categoryCode)
                || string.IsNullOrWhiteSpace(category))
            {
                return new JsonResult(new {
                    success = false,
                    message = "Missing creation data. Open the CreateTotal action/page to provide details."
                });
            }

            try
            {
                // Ensure category code is unique
                if (await NodeContext.Cash_tbCategories.AnyAsync(c => c.VersionId == versionId
                    && c.CategoryCode == categoryCode))
                    return new JsonResult(new { success = false, message = "Category code already exists." });

                var categoryCommand = new CategoryTreeCategoryCommand(versionId, categoryCode,
                    category, (short)NodeEnum.CategoryType.CashTotal,
                    cashPolarityCode ?? (short)NodeEnum.CashPolarity.Neutral,
                    cashTypeCode ?? 0, 0,
                    isEnabled.HasValue && isEnabled.Value == 0 ? (short)0 : (short)1,
                    "Total category created.");
                var totals = new List<CategoryTreeTotalCommand>();
                if (!string.IsNullOrWhiteSpace(parentKey))
                {
                    short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == versionId && t.ParentCode == parentKey)
                        .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);
                    totals.Add(new CategoryTreeTotalCommand(versionId, parentKey, categoryCode,
                        nextOrder, false, "Total category attached."));
                }
                await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(versionId,
                    new[] { categoryCommand }, totals, Array.Empty<CategoryTreeCashCodeMoveCommand>()));

                return new JsonResult(new { success = true, message = "Category created", key = categoryCode, parentKey = parentKey ?? "" });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return ChangeFailed(e);
            }
        }

        /// <summary>
        /// Create a new Cash Code under a category.
        /// Expects categoryCode, taxCode (or fallback), cashCode, cashDescription.
        /// Returns JSON: { success, message, cashCode }
        /// </summary>
        public async Task<JsonResult> OnPostCreateCodeByCategoryAsync(
            [FromForm] string versionId,
            [FromForm] string categoryCode,
            [FromForm] string taxCode,
            [FromForm] string cashCode,
            [FromForm] string cashDescription,
            [FromForm] string templateCode = null)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(categoryCode)
                || string.IsNullOrWhiteSpace(cashCode) || string.IsNullOrWhiteSpace(cashDescription))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                // Validate category exists and enabled
                var catExists = await NodeContext.Cash_tbCategories.AnyAsync(c => c.VersionId == versionId
                    && c.CategoryCode == categoryCode && c.IsEnabled != 0);
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

                await CategoryTreeService.CreateCashCodeAsync(new CategoryTreeCashCodeCreateCommand(
                    versionId, cashCode, cashDescription, categoryCode, finalTaxCode, 1));

                return new JsonResult(new { success = true, message = "Cash code created", cashCode = cashCode });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return ChangeFailed(e);
            }
        }

        /// <summary>
        /// Add an existing category as a child under a specified parent (totals).
        /// Ensures parent is CashTotal, prevents cycles and duplicate mappings.
        /// </summary>
        public async Task<JsonResult> OnPostAddExistingCategoryAsync([FromForm] string versionId,
            [FromForm] string parentKey, [FromForm] string childKey)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(parentKey)
                || string.IsNullOrWhiteSpace(childKey))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            if (string.Equals(parentKey, childKey, StringComparison.OrdinalIgnoreCase))
                return new JsonResult(new { success = false, message = "Parent and child cannot be the same." });

            try
            {
                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && c.CategoryCode == parentKey && c.IsEnabled != 0)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode })
                    .FirstOrDefaultAsync();

                if (parent == null)
                    return new JsonResult(new { success = false, message = "Parent category not found or disabled." });

                if (parent.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                    return new JsonResult(new { success = false, message = "Parent must be a Total-type category." });

                var child = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && c.CategoryCode == childKey && c.IsEnabled != 0)
                    .Select(c => c.CategoryCode)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(child))
                    return new JsonResult(new { success = false, message = "Child category not found or disabled." });

                var exists = await NodeContext.Cash_tbCategoryTotals.AnyAsync(t => t.VersionId == versionId
                    && t.ParentCode == parentKey && t.ChildCode == childKey);
                if (exists)
                    return new JsonResult(new { success = false, message = "Child already attached to parent." });

                // Prevent cycles (ensure parent is not a descendant of child)
                var parentMap = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == versionId && t.ChildCode != null && t.ParentCode != null)
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

                short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == versionId && t.ParentCode == parentKey)
                    .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                await CategoryTreeService.SaveTotalAsync(new CategoryTreeTotalCommand(versionId,
                    parentKey, childKey, nextOrder, false, "Existing category attached."));

                return new JsonResult(new { success = true, message = "Child added", key = childKey, parentKey });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return ChangeFailed(e);
            }
        }

        /// <summary>
        /// Add an existing Cash Code to a category (reassign code to the target category).
        /// </summary>
        public async Task<JsonResult> OnPostAddExistingCodeAsync([FromForm] string versionId,
            [FromForm] string parentKey, [FromForm] string codeKey)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(parentKey)
                || string.IsNullOrWhiteSpace(codeKey))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                var category = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == versionId && c.CategoryCode == parentKey && c.IsEnabled != 0)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode })
                    .FirstOrDefaultAsync();

                if (category == null)
                    return new JsonResult(new { success = false, message = "Target category not found or disabled." });

                // Optionally enforce category type (only allow attaching to CashCode categories).
                // This follows the UI rule that codes belong under CategoryType == CashCode (0).
                if (category.CategoryTypeCode != (short)NodeEnum.CategoryType.Nominal)
                    return new JsonResult(new { success = false, message = "Target category is not a Cash Code category." });

                var code = await NodeContext.Cash_tbCodes.FirstOrDefaultAsync(c => c.CashCode == codeKey);
                if (code == null)
                    return new JsonResult(new { success = false, message = "Cash code not found." });

                await CategoryTreeService.MoveCashCodeAsync(versionId, codeKey, parentKey,
                    "Cash Code moved.");

                return new JsonResult(new { success = true, message = "Cash code reassigned", cashCode = codeKey, parentKey });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return ChangeFailed(e);
            }
        }

        /// <summary>
        /// CreateCashCode shortcut - if only parentKey provided instruct client to open full create page.
        /// </summary>
        public Task<JsonResult> OnPostCreateCodeAsync([FromForm] string parentKey)
        {
            if (!IsAdmin())
                return Task.FromResult(new JsonResult(new { success = false, message = "Insufficient privileges" }));

            if (string.IsNullOrWhiteSpace(parentKey))
                return Task.FromResult(new JsonResult(new { success = false, message = "Missing parentKey. Open the Create Code page." }));

            return Task.FromResult(new JsonResult(new { success = false, message = "Open the CreateCashCode page to provide details.", parentKey }));
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

        public async Task<JsonResult> OnPostDeleteCashCodeAsync([FromForm] string versionId,
            [FromForm] string key)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(key)
                || !key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
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

                await CategoryTreeService.DeleteCashCodeAsync(versionId, cashCode,
                    "Cash Code deleted.");

                return new JsonResult(new { success = true, message = "Cash code deleted.", cashCode = cashCode });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return ChangeFailed(e);
            }
        }

        public async Task<JsonResult> OnPostDeleteCategoryAsync([FromForm] string versionId,
            [FromForm] string key)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(key))
                return new JsonResult(new { success = false, message = "Missing version or key" });

            try
            {
                // Verify category exists
                var cat = await NodeContext.Cash_tbCategories.FirstOrDefaultAsync(c =>
                    c.VersionId == versionId && c.CategoryCode == key);
                if (cat == null)
                    return new JsonResult(new { success = false, message = "Category not found." });

                // Build descendant set (recursive)
                var edges = await NodeContext.Cash_tbCategoryTotals
                                .Where(t => t.VersionId == versionId)
                                .Select(t => new { t.ParentCode, t.ChildCode, t.DisplayOrder })
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
                var codeCount = await NodeContext.Cash_tbCategoryCodeMaps
                    .Where(map => map.VersionId == versionId && seen.Contains(map.CategoryCode))
                    .CountAsync();
                if (codeCount > 0)
                {
                    return new JsonResult(new { success = false, message = "Cannot delete category tree while Cash Codes exist. Remove or reassign codes first.", codes = codeCount });
                }

                var edgeDeletes = edges
                    .Where(edge => seen.Contains(edge.ParentCode) || seen.Contains(edge.ChildCode))
                    .Select(edge => new CategoryTreeTotalCommand(versionId, edge.ParentCode,
                        edge.ChildCode, edge.DisplayOrder, true, "Category subtree removed."))
                    .ToArray();
                var deleteOrder = seen.OrderByDescending(code =>
                    edges.Count(edge => seen.Contains(edge.ParentCode)
                        && string.Equals(edge.ChildCode, code, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
                await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(versionId,
                    Array.Empty<CategoryTreeCategoryCommand>(), edgeDeletes,
                    Array.Empty<CategoryTreeCashCodeMoveCommand>(), deleteOrder));

                return new JsonResult(new { success = true, message = "Category deleted", key, deletedCount = seen.Count });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return ChangeFailed(e);
            }
        }

        public async Task<JsonResult> OnPostDeleteTotalAsync([FromForm] string versionId,
            [FromForm] string parentKey, [FromForm] string childKey)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(parentKey)
                || string.IsNullOrWhiteSpace(childKey))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                // Ensure mapping exists (defensive)
                var mappingExists = await NodeContext.Cash_tbCategoryTotals
                    .AnyAsync(t => t.VersionId == versionId
                        && t.ParentCode == parentKey && t.ChildCode == childKey);

                if (!mappingExists)
                    return new JsonResult(new { success = false, message = "Mapping not found." });

                // Build descendant set from childKey
                var edges = await NodeContext.Cash_tbCategoryTotals
                                .Where(t => t.VersionId == versionId)
                                .Select(t => new { t.ParentCode, t.ChildCode, t.DisplayOrder })
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
                    .Where(c => c.VersionId == versionId && seen.Contains(c.CategoryCode))
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

                var links = edges
                    .Where(t => totalsToDelete.Contains(t.ParentCode)
                        || totalsToDelete.Contains(t.ChildCode)
                        || (t.ParentCode == parentKey && t.ChildCode == childKey))
                    .Select(t => new CategoryTreeTotalCommand(versionId, t.ParentCode, t.ChildCode,
                        t.DisplayOrder, true, "Total category subtree removed."))
                    .ToArray();
                await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(versionId,
                    Array.Empty<CategoryTreeCategoryCommand>(), links,
                    Array.Empty<CategoryTreeCashCodeMoveCommand>(), totalsToDelete.ToArray()));
                var deletedTotals = totalsToDelete.Count;

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
                return ChangeFailed(ex);
            }
        }

        /// <summary>
        /// Create a category (used for disconnected creation or explicit category creation).
        /// Accepts parentKey, categoryCode, category, cashTypeCode, cashPolarityCode, isEnabled.
        /// If parentKey == DisconnectedNodeKey no totals mapping is created.
        /// Returns JSON: { success, message, key, parentKey }.
        /// </summary>
        public async Task<JsonResult> OnPostCreateCategoryAsync(
            [FromForm] string versionId,
            [FromForm] string parentKey,
            [FromForm] string categoryCode,
            [FromForm] string category,
            [FromForm] short? cashTypeCode,
            [FromForm] short? cashPolarityCode,
            [FromForm] short? isEnabled)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(categoryCode)
                || string.IsNullOrWhiteSpace(category))
                return new JsonResult(new { success = false, message = "Missing parameters." });

            try
            {
                if (await NodeContext.Cash_tbCategories.AnyAsync(c => c.VersionId == versionId
                    && c.CategoryCode == categoryCode))
                    return new JsonResult(new { success = false, message = "Category code already exists." });

                var categoryCommand = new CategoryTreeCategoryCommand(versionId, categoryCode,
                    category, (short)NodeEnum.CategoryType.CashTotal,
                    cashPolarityCode ?? (short)NodeEnum.CashPolarity.Neutral,
                    cashTypeCode ?? (short)NodeEnum.CashType.Trade, 0,
                    isEnabled.HasValue && isEnabled.Value == 0 ? (short)0 : (short)1,
                    "Category created.");
                var totals = new List<CategoryTreeTotalCommand>();

                if (!string.IsNullOrWhiteSpace(parentKey) && !string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == versionId && t.ParentCode == parentKey)
                        .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                    totals.Add(new CategoryTreeTotalCommand(versionId, parentKey, categoryCode,
                        nextOrder, false, "Category attached."));
                }
                await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(versionId,
                    new[] { categoryCommand }, totals, Array.Empty<CategoryTreeCashCodeMoveCommand>()));

                return new JsonResult(new { success = true, message = "Category created", key = categoryCode, parentKey = parentKey ?? "" });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                return ChangeFailed(ex);
            }
        }

        /// <summary>
        /// Clears the error state for an expression (Cash.tbCategoryExp.IsError and ErrorMessage).
        /// Expects key in the usual expression form (e.g. "expr:XYZ" or a raw CategoryCode).
        /// </summary>       
        public async Task<JsonResult> OnPostResetExpressionErrorAsync([FromForm] string key)
        {
            try
            {
                if (!User.IsInRole(Constants.AdministratorsRole))
                    return new JsonResult(new { success = false, message = "Insufficient privileges" });

                if (string.IsNullOrWhiteSpace(key))
                    return new JsonResult(new { success = false, message = "Missing key" });

                // Normalise "expr:CODE" to "CODE" if required
                string categoryCode = key;
                if (CategoryTreeModel.IsExpressionKey(key))
                    categoryCode = key.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);

                var exp = await NodeContext.Cash_tbCategoryExps
                    .SingleOrDefaultAsync(e => e.CategoryCode == categoryCode);

                if (exp == null)
                    return new JsonResult(new { success = false, message = "Expression not found." });

                exp.IsError = false;
                exp.ErrorMessage = null;

                await NodeContext.SaveChangesAsync();

                return new JsonResult(new {
                    success = true,
                    key = CategoryTreeModel.MakeExpressionKey(categoryCode),
                    isError = exp.IsError,
                    errorMessage = exp.ErrorMessage
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = e.Message });
            }
        }

        public async Task<JsonResult> OnPostUnlinkAsync([FromForm] string versionId,
            [FromForm] string parentKey, [FromForm] string childKey)
        {
            if (!IsAdmin())
                return new JsonResult(new { success = false, message = "Insufficient privileges" });

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(parentKey)
                || string.IsNullOrWhiteSpace(childKey))
                return new JsonResult(new { success = false, message = "Missing parameters" });

            try
            {
                var link = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == versionId
                        && t.ParentCode == parentKey && t.ChildCode == childKey)
                    .FirstOrDefaultAsync();

                if (link == null)
                    return new JsonResult(new { success = false, message = "Link not found" });

                var siblings = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == versionId && t.ParentCode == parentKey
                        && t.ChildCode != childKey)
                    .OrderBy(t => t.DisplayOrder)
                    .ToListAsync();

                var commands = new List<CategoryTreeTotalCommand>
                {
                    new(versionId, parentKey, childKey, link.DisplayOrder, true,
                        "Category unlinked.")
                };
                commands.AddRange(siblings.Select((sibling, index) =>
                    new CategoryTreeTotalCommand(versionId, parentKey, sibling.ChildCode,
                        (short)(index + 1), false, "Sibling order normalised.")));
                await ApplyTotalsAsync(versionId, commands);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                return ChangeFailed(ex);
            }
        }
        #endregion

        private static JsonResult ChangeFailed(Exception exception) => new(new {
            success = false,
            message = CategoryTreeCommandFeedback.From(exception)
        });
    }
}
