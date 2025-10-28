using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    /// <summary>
    /// Cash Type-based category ordering (DisplayOrder on Cash_tbCategories).
    /// Only applies to CategoryTypeCode == 0 (Cash Code).
    /// </summary>
    public partial class CategoryTreeModel
    {
        private const short CATEGORYTYPE_CASHCODE = 0;

        // -------- Helpers (no handlers in this file) --------

        private static bool IsTypeParent(string parentKey)
        {
            return !string.IsNullOrWhiteSpace(parentKey)
                   && parentKey.StartsWith("type:", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalize DisplayOrder for categories within a CashTypeCode (CategoryTypeCode == 0).
        /// Ensures strictly unique positive sequence for DisplayOrder > 0, leaves 0 values as is (uninitialized).
        /// Returns the max positive DisplayOrder after normalization.
        /// </summary>
        private async Task<short> NormalizeCategoryDisplayOrderByType(short cashTypeCode)
        {
            var list = await NodeContext.Cash_tbCategories
                .Where(c => c.CashTypeCode == cashTypeCode && c.CategoryTypeCode == CATEGORYTYPE_CASHCODE && c.DisplayOrder > 0)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryCode)
                .ToListAsync();

            short seq = 1;
            bool changed = false;
            foreach (var c in list)
            {
                if (c.DisplayOrder != seq)
                {
                    c.DisplayOrder = seq;
                    changed = true;
                }
                seq++;
            }

            if (changed)
                await NodeContext.SaveChangesAsync();

            return (short)(seq - 1);
        }

        /// <summary>
        /// Reorder a category within its CashTypeCode. 0-sorted last. If moving into or from 0,
        /// the category is assigned to the end of the initialized block (max+1).
        /// </summary>
        private async Task<JsonResult> ReorderCategoryByType(string categoryCode, bool moveUp)
        {
            var cat = await NodeContext.Cash_tbCategories
                .Where(c => c.CategoryCode == categoryCode)
                .Select(c => new
                {
                    c.CategoryCode,
                    c.DisplayOrder,
                    c.CashTypeCode,
                    c.CategoryTypeCode
                })
                .FirstOrDefaultAsync();

            if (cat == null)
                return new JsonResult(new { success = false, message = "Category not found" });

            if (cat.CategoryTypeCode != CATEGORYTYPE_CASHCODE)
                return new JsonResult(new { success = false, message = "Only Cash Code categories can be reordered here" });

            short maxPos = await NormalizeCategoryDisplayOrderByType(cat.CashTypeCode);

            var all = await NodeContext.Cash_tbCategories
                .Where(c => c.CashTypeCode == cat.CashTypeCode && c.CategoryTypeCode == CATEGORYTYPE_CASHCODE)
                .Select(c => new { c.CategoryCode, c.DisplayOrder })
                .ToListAsync();

            var positives = all
                .Where(x => x.DisplayOrder > 0)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            var zerosExist = all.Any(x => x.DisplayOrder == 0);

            var current = all.First(x => x.CategoryCode == categoryCode);
            if (current.DisplayOrder == 0)
            {
                var tracked = await NodeContext.Cash_tbCategories.SingleAsync(c => c.CategoryCode == categoryCode);
                tracked.DisplayOrder = (short)(maxPos + 1);
                await NodeContext.SaveChangesAsync();
                return new JsonResult(new { success = true, message = "Assigned to end of list", categoryCode, displayOrder = tracked.DisplayOrder });
            }

            int index = positives.FindIndex(x => x.CategoryCode == categoryCode);
            if (index < 0)
                return new JsonResult(new { success = false, message = "Reorder failed: item not found in sequence" });

            if (moveUp)
            {
                if (index == 0)
                    return new JsonResult(new { success = true, message = "Already at top" });

                var prev = positives[index - 1];

                var curTracked = await NodeContext.Cash_tbCategories.SingleAsync(c => c.CategoryCode == categoryCode);
                var prevTracked = await NodeContext.Cash_tbCategories.SingleAsync(c => c.CategoryCode == prev.CategoryCode);

                short tmp = curTracked.DisplayOrder;
                curTracked.DisplayOrder = prevTracked.DisplayOrder;
                prevTracked.DisplayOrder = tmp;

                await NodeContext.SaveChangesAsync();
                return new JsonResult(new { success = true, moved = "up", categoryCode, displayOrder = curTracked.DisplayOrder });
            }
            else
            {
                if (index < positives.Count - 1)
                {
                    var next = positives[index + 1];

                    var curTracked = await NodeContext.Cash_tbCategories.SingleAsync(c => c.CategoryCode == categoryCode);
                    var nextTracked = await NodeContext.Cash_tbCategories.SingleAsync(c => c.CategoryCode == next.CategoryCode);

                    short tmp = curTracked.DisplayOrder;
                    curTracked.DisplayOrder = nextTracked.DisplayOrder;
                    nextTracked.DisplayOrder = tmp;

                    await NodeContext.SaveChangesAsync();
                    return new JsonResult(new { success = true, moved = "down", categoryCode, displayOrder = curTracked.DisplayOrder });
                }

                if (zerosExist)
                {
                    var curTracked = await NodeContext.Cash_tbCategories.SingleAsync(c => c.CategoryCode == categoryCode);
                    curTracked.DisplayOrder = (short)(maxPos + 1);
                    await NodeContext.SaveChangesAsync();
                    return new JsonResult(new { success = true, moved = "down", categoryCode, displayOrder = curTracked.DisplayOrder });
                }

                return new JsonResult(new { success = true, message = "Already at bottom" });
            }
        }

        // Admin-only: normalize all Cash Types (CategoryTypeCode == 0)
        public async Task<JsonResult> OnPostUpgradeTypeOrderingAsync()
        {
            try
            {
                if (!IsAdmin())
                    return new JsonResult(new { success = false, message = "Insufficient privileges" });

                var types = await NodeContext.Cash_tbTypes
                    .Select(t => t.CashTypeCode)
                    .ToListAsync();

                int normalizedTypes = 0;
                foreach (var t in types)
                {
                    var maxPos = await NormalizeCategoryDisplayOrderByType(t);
                    normalizedTypes++;
                }

                return new JsonResult(new { success = true, normalizedTypes });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = e.Message });
            }
        }
    }
}