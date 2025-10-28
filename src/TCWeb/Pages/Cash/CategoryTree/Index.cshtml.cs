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

namespace TradeControl.Web.Pages.Cash.CategoryCode
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
        /// Moves a node up within its sibling sequence (root, normal, or disconnected set),
        /// enforcing non-crossing of CashPolarityCode boundaries.
        /// Also supports Cash Type subtree reordering via DisplayOrder on Cash_tbCategories.
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
                // Cash Type subtree: delegate to per-type ordering
                if (IsTypeParent(parentKey))
                    return await ReorderCategoryByType(key, moveUp: true);

                // Ensure the working set has a persisted order (non-type sets only)
                if (!await IsDisplayOrderInitialised(parentKey))
                    await DisplayOrderInitialise(parentKey);

                // Disconnected: reorder by category.DisplayOrder
                if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var list = await NodeContext.Cash_tbCategories
                                    .Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
                                    .OrderBy(c => c.DisplayOrder)
                                    .ToListAsync();

                    var idx = list.FindIndex(c => c.CategoryCode == key);
                    if (idx < 0) return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx == 0) return new JsonResult(new { success = false, message = "Already at top" });

                    var cur = list[idx];
                    var prev = list[idx - 1];
                    if (cur.CashPolarityCode != prev.CashPolarityCode)
                        return new JsonResult(new { success = false, message = "Cannot move across polarity boundary" });

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == prev.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, prev.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }

                // Root-level: reorder by category.DisplayOrder
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
                    if (idx < 0) return new JsonResult(new { success = false, message = "Node not found in root set" });
                    if (idx == 0) return new JsonResult(new { success = false, message = "Already at top" });

                    var cur = roots[idx];
                    var prev = roots[idx - 1];
                    if (cur.CashPolarityCode != prev.CashPolarityCode)
                        return new JsonResult(new { success = false, message = "Cannot move across polarity boundary" });

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == prev.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, prev.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }
                else
                {
                    // Child-level: reorder by totals.DisplayOrder
                    var totalsForParent = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey)
                        .OrderBy(t => t.DisplayOrder)
                        .ToListAsync();

                    var idx = totalsForParent.FindIndex(t => t.ChildCode == key);
                    if (idx < 0) return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx == 0) return new JsonResult(new { success = false, message = "Already at top" });

                    var cur = totalsForParent[idx];
                    var prev = totalsForParent[idx - 1];

                    var cats = await NodeContext.Cash_tbCategories
                        .Where(c => c.CategoryCode == cur.ChildCode || c.CategoryCode == prev.ChildCode)
                        .Select(c => new { c.CategoryCode, c.CashPolarityCode })
                        .ToListAsync();

                    var curPol = cats.First(c => c.CategoryCode == cur.ChildCode).CashPolarityCode;
                    var prevPol = cats.First(c => c.CategoryCode == prev.ChildCode).CashPolarityCode;
                    if (curPol != prevPol)
                        return new JsonResult(new { success = false, message = "Cannot move across polarity boundary" });

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
        /// Moves a node down within its sibling sequence (root, normal, or disconnected set),
        /// enforcing non-crossing of CashPolarityCode boundaries.
        /// Also supports Cash Type subtree reordering via DisplayOrder on Cash_tbCategories.
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
                // Cash Type subtree: delegate to per-type ordering
                if (IsTypeParent(parentKey))
                    return await ReorderCategoryByType(key, moveUp: false);

                // Ensure the working set has a persisted order (non-type sets only)
                if (!await IsDisplayOrderInitialised(parentKey))
                    await DisplayOrderInitialise(parentKey);

                // Disconnected: reorder by category.DisplayOrder
                if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var list = await NodeContext.Cash_tbCategories
                                    .Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
                                    .OrderBy(c => c.DisplayOrder)
                                    .ToListAsync();

                    var idx = list.FindIndex(c => c.CategoryCode == key);
                    if (idx < 0) return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx >= list.Count - 1) return new JsonResult(new { success = false, message = "Already at bottom" });

                    var cur = list[idx];
                    var next = list[idx + 1];
                    if (cur.CashPolarityCode != next.CashPolarityCode)
                        return new JsonResult(new { success = false, message = "Cannot move across polarity boundary" });

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == next.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, next.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }

                // Root-level: reorder by category.DisplayOrder
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
                    if (idx < 0) return new JsonResult(new { success = false, message = "Node not found in root set" });
                    if (idx >= roots.Count - 1) return new JsonResult(new { success = false, message = "Already at bottom" });

                    var cur = roots[idx];
                    var next = roots[idx + 1];
                    if (cur.CashPolarityCode != next.CashPolarityCode)
                        return new JsonResult(new { success = false, message = "Cannot move across polarity boundary" });

                    await using var tx = await NodeContext.Database.BeginTransactionAsync();
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == next.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, cur.DisplayOrder));
                    await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == cur.CategoryCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, next.DisplayOrder));
                    await tx.CommitAsync();

                    return new JsonResult(new { success = true });
                }
                else
                {
                    // Child-level: reorder by totals.DisplayOrder
                    var totalsForParent = await NodeContext.Cash_tbCategoryTotals
                                    .Where(t => t.ParentCode == parentKey)
                                    .OrderBy(t => t.DisplayOrder)
                                    .ToListAsync();

                    var idx = totalsForParent.FindIndex(t => t.ChildCode == key);
                    if (idx < 0) return new JsonResult(new { success = false, message = "Node not found" });
                    if (idx >= totalsForParent.Count - 1) return new JsonResult(new { success = false, message = "Already at bottom" });

                    var cur = totalsForParent[idx];
                    var next = totalsForParent[idx + 1];

                    var cats = await NodeContext.Cash_tbCategories
                        .Where(c => c.CategoryCode == cur.ChildCode || c.CategoryCode == next.ChildCode)
                        .Select(c => new { c.CategoryCode, c.CashPolarityCode })
                        .ToListAsync();

                    var curPol = cats.First(c => c.CategoryCode == cur.ChildCode).CashPolarityCode;
                    var nextPol = cats.First(c => c.CategoryCode == next.ChildCode).CashPolarityCode;
                    if (curPol != nextPol)
                        return new JsonResult(new { success = false, message = "Cannot move across polarity boundary" });

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
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }
        }
    }
}