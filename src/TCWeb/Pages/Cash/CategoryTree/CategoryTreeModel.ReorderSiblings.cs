using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Authorization;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public partial class CategoryTreeModel
    {
        /// <summary>
        /// Reorders a category among its siblings relative to an anchor sibling.
        /// Mode: "before" or "after" the anchor. Polarity boundary constraint removed.
        /// Works for:
        ///  - Root-level categories (those that are linked and not children in totals)
        ///  - Disconnected categories (unmapped, enabled)
        ///  - Child categories under a parent (totals mapping)
        /// </summary>
        /// <param name="parentKey">Parent context key; __ROOT__ for root grouping, __DISCONNECTED__ for disconnected set.</param>
        /// <param name="key">Category code to move.</param>
        /// <param name="anchorKey">Existing sibling to position relative to.</param>
        /// <param name="mode">"before" or "after".</param>
        public async Task<JsonResult> OnPostReorderSiblingsAsync(
            [FromForm] string parentKey,
            [FromForm] string key,
            [FromForm] string anchorKey,
            [FromForm] string mode)
        {
            if (!IsAdmin())
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }

            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(anchorKey) ||
                string.IsNullOrWhiteSpace(parentKey))
            {
                return new JsonResult(new { success = false, message = "Missing parameters." });
            }

            mode = (mode ?? "").ToLowerInvariant();
            if (mode != "before" && mode != "after")
            {
                return new JsonResult(new { success = false, message = "Invalid mode." });
            }

            try
            {
                // Root-level: reorder Cash_tbCategories.DisplayOrder for top-level linked categories
                if (IsRootKey(parentKey))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var childCodes = new System.Collections.Generic.HashSet<string>(
                        totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));

                    var linkedSet = new System.Collections.Generic.HashSet<string>(
                        totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var list = await NodeContext.Cash_tbCategories
                        .Where(c =>
                            !childCodes.Contains(c.CategoryCode) &&
                            c.IsEnabled != 0 &&
                            linkedSet.Contains(c.CategoryCode))
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.CategoryCode)
                        .Select(c => new { c.CategoryCode, c.DisplayOrder })
                        .ToListAsync();

                    if (!list.Any(x => x.CategoryCode == key) || !list.Any(x => x.CategoryCode == anchorKey))
                    {
                        return new JsonResult(new { success = false, message = "Nodes not in root set" });
                    }

                    var seq = list.Select(x => x.CategoryCode).ToList();
                    seq.Remove(key);

                    var insertIdx = seq.IndexOf(anchorKey);
                    if (insertIdx < 0) insertIdx = 0;
                    if (mode == "after") insertIdx++;
                    if (insertIdx < 0) insertIdx = 0;
                    if (insertIdx > seq.Count) insertIdx = seq.Count;
                    seq.Insert(insertIdx, key);

                    await using (var tx = await NodeContext.Database.BeginTransactionAsync())
                    {
                        short order = 1;
                        foreach (var code in seq)
                        {
                            await NodeContext.Cash_tbCategories
                                .Where(c => c.CategoryCode == code)
                                .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, order));
                            order++;
                        }
                        await tx.CommitAsync();
                    }

                    return new JsonResult(new { success = true });
                }

                // Disconnected: reorder enabled categories not referenced in totals
                if (string.Equals(parentKey, DisconnectedNodeKey, System.StringComparison.Ordinal))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
                    var linkedSet = new System.Collections.Generic.HashSet<string>(
                        totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

                    var list = await NodeContext.Cash_tbCategories
                        .Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.CategoryCode)
                        .Select(c => new { c.CategoryCode, c.DisplayOrder })
                        .ToListAsync();

                    if (!list.Any(x => x.CategoryCode == key) || !list.Any(x => x.CategoryCode == anchorKey))
                    {
                        return new JsonResult(new { success = false, message = "Nodes not in disconnected set" });
                    }

                    var seq = list.Select(x => x.CategoryCode).ToList();
                    seq.Remove(key);

                    var insertIdx = seq.IndexOf(anchorKey);
                    if (insertIdx < 0) insertIdx = 0;
                    if (mode == "after") insertIdx++;
                    if (insertIdx < 0) insertIdx = 0;
                    if (insertIdx > seq.Count) insertIdx = seq.Count;
                    seq.Insert(insertIdx, key);

                    await using (var tx = await NodeContext.Database.BeginTransactionAsync())
                    {
                        short order = 1;
                        foreach (var code in seq)
                        {
                            await NodeContext.Cash_tbCategories
                                .Where(c => c.CategoryCode == code)
                                .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, order));
                            order++;
                        }
                        await tx.CommitAsync();
                    }

                    return new JsonResult(new { success = true });
                }

                // Child totals under a parent: reorder Cash_tbCategoryTotals.DisplayOrder
                {
                    var tots = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == parentKey)
                        .OrderBy(t => t.DisplayOrder)
                        .Select(t => new { t.ChildCode, t.DisplayOrder })
                        .ToListAsync();

                    if (!tots.Any(x => x.ChildCode == key) || !tots.Any(x => x.ChildCode == anchorKey))
                    {
                        return new JsonResult(new { success = false, message = "Nodes not found under parent" });
                    }

                    var seq = tots.Select(x => x.ChildCode).ToList();
                    seq.Remove(key);

                    var insertIdx = seq.IndexOf(anchorKey);
                    if (insertIdx < 0) insertIdx = 0;
                    if (mode == "after") insertIdx++;
                    if (insertIdx < 0) insertIdx = 0;
                    if (insertIdx > seq.Count) insertIdx = seq.Count;
                    seq.Insert(insertIdx, key);

                    await using (var tx = await NodeContext.Database.BeginTransactionAsync())
                    {
                        short order = 1;
                        foreach (var code in seq)
                        {
                            await NodeContext.Cash_tbCategoryTotals
                                .Where(t => t.ParentCode == parentKey && t.ChildCode == code)
                                .ExecuteUpdateAsync(s => s.SetProperty(t => t.DisplayOrder, order));
                            order++;
                        }
                        await tx.CommitAsync();
                    }

                    return new JsonResult(new { success = true });
                }
            }
            catch (System.Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }
    }
}
