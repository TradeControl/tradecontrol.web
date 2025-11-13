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
    }
}
