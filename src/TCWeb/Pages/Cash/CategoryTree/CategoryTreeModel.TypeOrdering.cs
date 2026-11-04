using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.AppServices.CategoryTree;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public partial class CategoryTreeModel
    {
        private const short CATEGORYTYPE_CASHCODE = 0;

        private static bool IsTypeParent(string parentKey)
        {
            return !string.IsNullOrWhiteSpace(parentKey)
                && parentKey.StartsWith("type:", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<JsonResult> ReorderCategoryByType(string versionId, string categoryCode,
            bool moveUp)
        {
            var category = await NodeContext.Cash_tbCategories
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.VersionId == versionId
                    && item.CategoryCode == categoryCode);
            if (category is null)
            {
                return new JsonResult(new { success = false, message = "Category not found" });
            }

            if (category.CategoryTypeCode != CATEGORYTYPE_CASHCODE)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Only Cash Code categories can be reordered here"
                });
            }

            var sequence = await NodeContext.Cash_tbCategories
                .AsNoTracking()
                .Where(item => item.VersionId == versionId
                    && item.CashTypeCode == category.CashTypeCode
                    && item.CategoryTypeCode == CATEGORYTYPE_CASHCODE)
                .OrderBy(item => item.DisplayOrder == 0)
                .ThenBy(item => item.DisplayOrder)
                .ThenBy(item => item.CategoryCode)
                .Select(item => item.CategoryCode)
                .ToListAsync();

            var index = sequence.IndexOf(categoryCode);
            var target = moveUp ? index - 1 : index + 1;
            if (index < 0 || target < 0 || target >= sequence.Count)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = moveUp ? "Already at top" : "Already at bottom"
                });
            }

            (sequence[index], sequence[target]) = (sequence[target], sequence[index]);
            await SaveCategoryOrdersAsync(versionId,
                sequence.Select((code, order) => (code, (short)(order + 1))),
                "Cash Type categories reordered.");
            return new JsonResult(new { success = true });
        }

        public async Task<JsonResult> OnPostUpgradeTypeOrderingAsync([FromForm] string versionId)
        {
            if (!IsAdmin())
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }

            if (string.IsNullOrWhiteSpace(versionId))
            {
                return new JsonResult(new { success = false, message = "Missing version." });
            }

            try
            {
                var groups = await NodeContext.Cash_tbCategories
                    .AsNoTracking()
                    .Where(item => item.VersionId == versionId
                        && item.CategoryTypeCode == CATEGORYTYPE_CASHCODE)
                    .OrderBy(item => item.CashTypeCode)
                    .ThenBy(item => item.DisplayOrder == 0)
                    .ThenBy(item => item.DisplayOrder)
                    .ThenBy(item => item.CategoryCode)
                    .Select(item => new { item.CashTypeCode, item.CategoryCode })
                    .ToListAsync();

                var orders = groups
                    .GroupBy(item => item.CashTypeCode)
                    .SelectMany(group => group.Select((item, order) =>
                        (item.CategoryCode, (short)(order + 1))))
                    .ToArray();
                await SaveCategoryOrdersAsync(versionId, orders,
                    "Cash Type category ordering normalised.");
                return new JsonResult(new
                {
                    success = true,
                    normalizedTypes = groups.Select(item => item.CashTypeCode).Distinct().Count()
                });
            }
            catch (Exception exception)
            {
                await NodeContext.ErrorLog(exception);
                return new JsonResult(new { success = false, message = exception.Message });
            }
        }

        public async Task<JsonResult> OnPostReorderTypeAsync([FromForm] string versionId,
            [FromForm] string key, [FromForm] string anchorKey, [FromForm] string mode)
        {
            if (!IsAdmin())
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(key)
                || string.IsNullOrWhiteSpace(anchorKey))
            {
                return new JsonResult(new { success = false, message = "Missing parameters." });
            }

            mode = (mode ?? string.Empty).ToLowerInvariant();
            if (mode != "before" && mode != "after")
            {
                return new JsonResult(new { success = false, message = "Invalid mode." });
            }

            try
            {
                var pair = await NodeContext.Cash_tbCategories
                    .AsNoTracking()
                    .Where(item => item.VersionId == versionId
                        && (item.CategoryCode == key || item.CategoryCode == anchorKey)
                        && item.IsEnabled != 0
                        && item.CategoryTypeCode == CATEGORYTYPE_CASHCODE)
                    .Select(item => new { item.CategoryCode, item.CashTypeCode })
                    .ToListAsync();
                if (pair.Count != 2 || pair[0].CashTypeCode != pair[1].CashTypeCode)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Categories are unavailable or not in the same Cash Type."
                    });
                }

                var cashTypeCode = pair[0].CashTypeCode;
                var sequence = await NodeContext.Cash_tbCategories
                    .AsNoTracking()
                    .Where(item => item.VersionId == versionId && item.IsEnabled != 0
                        && item.CategoryTypeCode == CATEGORYTYPE_CASHCODE
                        && item.CashTypeCode == cashTypeCode)
                    .OrderBy(item => item.DisplayOrder == 0)
                    .ThenBy(item => item.DisplayOrder)
                    .ThenBy(item => item.CategoryCode)
                    .Select(item => item.CategoryCode)
                    .ToListAsync();

                sequence.Remove(key);
                var insertIndex = sequence.IndexOf(anchorKey);
                if (mode == "after")
                {
                    insertIndex++;
                }
                sequence.Insert(Math.Clamp(insertIndex, 0, sequence.Count), key);

                await SaveCategoryOrdersAsync(versionId,
                    sequence.Select((code, order) => (code, (short)(order + 1))),
                    "Cash Type categories reordered.");
                return new JsonResult(new { success = true });
            }
            catch (Exception exception)
            {
                await NodeContext.ErrorLog(exception);
                return new JsonResult(new {
                    success = false,
                    message = CategoryTreeCommandFeedback.From(exception)
                });
            }
        }
    }
}
