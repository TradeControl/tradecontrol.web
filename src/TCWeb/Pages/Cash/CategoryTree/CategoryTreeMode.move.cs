using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Authorization;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public partial class CategoryTreeModel
    {
        private async Task<HashSet<string>> GetRootAncestorsAsync(string key)
        {
            var edges = await NodeContext.Cash_tbCategoryTotals
                .AsNoTracking()
                .Where(t => t.ParentCode != null && t.ChildCode != null)
                .Select(t => new { t.ParentCode, t.ChildCode })
                .ToListAsync();

            // Build lookup: child -> parents (multi-parent supported)
            var parentsByChild = edges
                .GroupBy(e => e.ChildCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ParentCode).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<string>();
            stack.Push(key);

            var guard = 0;
            while (stack.Count > 0 && guard++ < 4096)
            {
                var cur = stack.Pop();
                if (!seen.Add(cur))
                    continue;

                if (!parentsByChild.TryGetValue(cur, out var parents) || parents.Count == 0)
                {
                    // No parent edges => root in totals graph
                    roots.Add(cur);
                    continue;
                }

                foreach (var p in parents)
                    stack.Push(p);
            }

            return roots;
        }

        public async Task<JsonResult> OnPostMoveAsync([FromForm] string key, [FromForm] string targetParentKey)
        {
            if (!User.IsInRole(Constants.AdministratorsRole))
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(targetParentKey))
            {
                return new JsonResult(new { success = false, message = "Missing parameters." });
            }
            if (string.Equals(key, targetParentKey, StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { success = false, message = "Invalid target." });
            }

            try
            {
                if (IsTypeParent(targetParentKey))
                {
                    return new JsonResult(new { success = false, message = "Move not allowed in Cash Type view." });
                }

                var cats = await NodeContext.Cash_tbCategories
                    .Where(c => (c.CategoryCode == key || c.CategoryCode == targetParentKey) && c.IsEnabled != 0)
                    .Select(c => c.CategoryCode)
                    .ToListAsync();

                if (!cats.Contains(key) || !cats.Contains(targetParentKey))
                {
                    return new JsonResult(new { success = false, message = "Category not found or disabled." });
                }

                // If already attached to target parent, do nothing
                var alreadyAttached = await NodeContext.Cash_tbCategoryTotals
                    .AnyAsync(t => t.ParentCode == targetParentKey && t.ChildCode == key);

                if (alreadyAttached)
                {
                    return new JsonResult(new { success = true, oldParentKey = string.Empty, newParentKey = targetParentKey });
                }

                // Compute whether key and targetParentKey are in the same totals tree (same root ancestor)
                var keyRoots = await GetRootAncestorsAsync(key);
                var targetRoots = await GetRootAncestorsAsync(targetParentKey);
                bool sameTree = keyRoots.Overlaps(targetRoots);

                // Find an old parent link to remove *only if* this is a move within the same tree
                string oldParentKey = null;
                if (sameTree)
                {
                    oldParentKey = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ChildCode == key)
                        .OrderBy(t => t.ParentCode)
                        .Select(t => t.ParentCode)
                        .FirstOrDefaultAsync();
                }

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                if (sameTree && !string.IsNullOrWhiteSpace(oldParentKey))
                {
                    // Remove only the specific edge we are moving from (do not delete all other memberships)
                    await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == oldParentKey && t.ChildCode == key)
                        .ExecuteDeleteAsync();
                }

                short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == targetParentKey)
                    .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal {
                    ParentCode = targetParentKey,
                    ChildCode = key,
                    DisplayOrder = nextOrder
                });

                await NodeContext.SaveChangesAsync();
                await tx.CommitAsync();

                return new JsonResult(new {
                    success = true,
                    oldParentKey = oldParentKey ?? string.Empty,
                    newParentKey = targetParentKey,
                    mode = sameTree ? "move" : "add"
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }
    }
}
