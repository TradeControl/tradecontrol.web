using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    /// <summary>
    /// PageModel that provides the server-side handlers used by the Cash Category Fancytree.
    /// Read access allowed to authenticated users; write actions enforce admin role checks.
    /// </summary>
    [Authorize]
    public partial class CategoryTreeModel : DI_BasePageModel
    {
        /// <summary>
        /// Special key used by the tree to represent the Disconnected categories folder (categories not referenced in totals).
        /// </summary>
        public const string DisconnectedNodeKey = "__DISCONNECTED__";

        /// <summary>
        /// Special key used by the tree to represent the visible root container node.
        /// </summary>
        public const string RootNodeKey = "__ROOT__";

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryTreeModel"/> class.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        public CategoryTreeModel(NodeContext context) : base(context) { }

        /// <summary>
        /// GET handler used to serve the page.
        /// </summary>
        public async Task OnGetAsync()
        {
            await SetViewData();
        }

        // --- Display Order Management ---

        /// <summary>
        /// Returns true if the <paramref name="parentKey"/> represents the root-level working set.
        /// </summary>
        private static bool IsRootKey(string parentKey) =>
            string.IsNullOrEmpty(parentKey) || string.Equals(parentKey, RootNodeKey, StringComparison.Ordinal);

        /// <summary>
        /// Moves a node up within its sibling sequence (root, normal, or disconnected set).
        /// Cash Polarity boundaries are no longer enforced (requested change).
        /// </summary>
        public async Task<JsonResult> OnPostMoveUpAsync([FromForm] string key, [FromForm] string parentKey)
        {
            if (!User.IsInRole(Constants.AdministratorsRole))
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            if (string.IsNullOrEmpty(key))
                return new JsonResult(new { success = false, message = "Missing key" });
            if (key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
                return new JsonResult(new { success = false, message = "Codes are not reorderable" });

            try
            {
                if (IsTypeParent(parentKey))
                    return await ReorderCategoryByType(key, moveUp: true);

                if (!await IsDisplayOrderInitialised(parentKey))
                    await DisplayOrderInitialise(parentKey);

                // Disconnected set
                if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var list = await NodeContext.Cash_tbCategories
                        .Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
                        .OrderBy(c => c.DisplayOrder)
                        .ToListAsync();

                    var idx = list.FindIndex(c => c.CategoryCode == key);
                    if (idx < 0)
                        return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx == 0)
                        return new JsonResult(new { success = false, message = "Already at top" });

                    var cur = list[idx];
                    var prev = list[idx - 1];

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == prev.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, prev.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }

                // Root set
                if (IsRootKey(parentKey))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
                    var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var roots = await NodeContext.Cash_tbCategories
                        .Where(c => !childCodes.Contains(c.CategoryCode) && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
                        .OrderBy(c => c.DisplayOrder)
                        .ToListAsync();

                    var idx = roots.FindIndex(c => c.CategoryCode == key);
                    if (idx < 0)
                        return new JsonResult(new { success = false, message = "Node not found in root set" });
                    if (idx == 0)
                        return new JsonResult(new { success = false, message = "Already at top" });

                    var cur = roots[idx];
                    var prev = roots[idx - 1];

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == prev.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, prev.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }

                // Child-level (totals mappings)
                {
                    var totalsForParent = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey)
                        .OrderBy(t => t.DisplayOrder)
                        .ToListAsync();

                    var idx = totalsForParent.FindIndex(t => t.ChildCode == key);
                    if (idx < 0)
                        return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx == 0)
                        return new JsonResult(new { success = false, message = "Already at top" });

                    var cur = totalsForParent[idx];
                    var prev = totalsForParent[idx - 1];

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey && t.ChildCode == prev.ChildCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(t => t.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey && t.ChildCode == cur.ChildCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(t => t.DisplayOrder, prev.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Moves a node down within its sibling sequence (root, normal, or disconnected set).
        /// Cash Polarity boundaries are no longer enforced (requested change).
        /// </summary>
        public async Task<JsonResult> OnPostMoveDownAsync([FromForm] string key, [FromForm] string parentKey)
        {
            if (!User.IsInRole(Constants.AdministratorsRole))
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            if (string.IsNullOrEmpty(key))
                return new JsonResult(new { success = false, message = "Missing key" });
            if (key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
                return new JsonResult(new { success = false, message = "Codes are not reorderable" });

            try
            {
                if (IsTypeParent(parentKey))
                    return await ReorderCategoryByType(key, moveUp: false);

                if (!await IsDisplayOrderInitialised(parentKey))
                    await DisplayOrderInitialise(parentKey);

                // Disconnected
                if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var list = await NodeContext.Cash_tbCategories
                        .Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
                        .OrderBy(c => c.DisplayOrder)
                        .ToListAsync();

                    var idx = list.FindIndex(c => c.CategoryCode == key);
                    if (idx < 0)
                        return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx >= list.Count - 1)
                        return new JsonResult(new { success = false, message = "Already at bottom" });

                    var cur = list[idx];
                    var next = list[idx + 1];

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == next.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, next.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }

                // Root
                if (IsRootKey(parentKey))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
                    var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var roots = await NodeContext.Cash_tbCategories
                        .Where(c => !childCodes.Contains(c.CategoryCode) && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
                        .OrderBy(c => c.DisplayOrder)
                        .ToListAsync();

                    var idx = roots.FindIndex(c => c.CategoryCode == key);
                    if (idx < 0)
                        return new JsonResult(new { success = false, message = "Node not found in root set" });
                    if (idx >= roots.Count - 1)
                        return new JsonResult(new { success = false, message = "Already at bottom" });

                    var cur = roots[idx];
                    var next = roots[idx + 1];

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == next.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, next.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }

                // Child-level
                {
                    var totalsForParent = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey)
                        .OrderBy(t => t.DisplayOrder)
                        .ToListAsync();

                    var idx = totalsForParent.FindIndex(t => t.ChildCode == key);
                    if (idx < 0)
                        return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx >= totalsForParent.Count - 1)
                        return new JsonResult(new { success = false, message = "Already at bottom" });

                    var cur = totalsForParent[idx];
                    var next = totalsForParent[idx + 1];

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey && t.ChildCode == next.ChildCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(t => t.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey && t.ChildCode == cur.ChildCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(t => t.DisplayOrder, next.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = e.Message });
            }
        }

        public async Task<JsonResult> OnPostReorderExpressionAsync([FromForm] string key, [FromForm] string anchorKey, [FromForm] string mode)
        {
            if (!User.IsInRole(Constants.AdministratorsRole))
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(anchorKey) || string.IsNullOrWhiteSpace(mode))
            {
                return new JsonResult(new { success = false, message = "Missing parameters" });
            }

            // Strip expr: if present
            if (CategoryTreeModel.IsExpressionKey(key))
            {
                key = key.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);
            }
            if (CategoryTreeModel.IsExpressionKey(anchorKey))
            {
                anchorKey = anchorKey.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);
            }

            try
            {
                // Fetch both categories
                var left = await NodeContext.Cash_tbCategories.FirstOrDefaultAsync(c => c.CategoryCode == key);
                var anchor = await NodeContext.Cash_tbCategories.FirstOrDefaultAsync(c => c.CategoryCode == anchorKey);

                if (left == null || anchor == null)
                {
                    return new JsonResult(new { success = false, message = "Category not found" });
                }

                short exprType = (short)NodeEnum.CategoryType.Expression;
                if (left.CategoryTypeCode != exprType || anchor.CategoryTypeCode != exprType)
                {
                    return new JsonResult(new { success = false, message = "Both nodes must be Expression categories" });
                }

                // Normalise display order sequence if duplicates or zeros exist.
                var allExpr = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryTypeCode == exprType)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                short rebuild = 1;
                foreach (var c in allExpr)
                {
                    if (c.DisplayOrder != rebuild)
                    {
                        c.DisplayOrder = rebuild;
                    }
                    rebuild++;
                }
                await NodeContext.SaveChangesAsync();

                // Reload entities after potential normalisation
                left = allExpr.First(c => c.CategoryCode == left.CategoryCode);
                anchor = allExpr.First(c => c.CategoryCode == anchor.CategoryCode);

                // Reorder: mode = before | after relative to anchor
                if (mode != "before" && mode != "after")
                {
                    return new JsonResult(new { success = false, message = "Invalid mode" });
                }

                if (left.CategoryCode == anchor.CategoryCode)
                {
                    return new JsonResult(new { success = false, message = "Same node" });
                }

                // Remove 'left' from sequence then re-insert before/after anchor
                var seq = allExpr.Where(c => c.CategoryCode != left.CategoryCode).ToList();
                var anchorIndex = seq.FindIndex(c => c.CategoryCode == anchor.CategoryCode);

                if (anchorIndex < 0)
                {
                    return new JsonResult(new { success = false, message = "Anchor not in sequence" });
                }

                var insertIndex = mode == "before" ? anchorIndex : anchorIndex + 1;
                if (insertIndex < 0) insertIndex = 0;
                if (insertIndex > seq.Count) insertIndex = seq.Count;
                seq.Insert(insertIndex, left);

                short order = 1;
                foreach (var c in seq)
                {
                    if (c.DisplayOrder != order)
                    {
                        await NodeContext.Cash_tbCategories
                            .Where(x => x.CategoryCode == c.CategoryCode)
                            .ExecuteUpdateAsync(s => s.SetProperty(x => x.DisplayOrder, order));
                    }
                    order++;
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
