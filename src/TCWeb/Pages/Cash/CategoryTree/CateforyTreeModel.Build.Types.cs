using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public partial class CategoryTreeModel
    {
        private const string TypesRootKey = "__TYPE_ROOT__";

        private static bool IsTypesRootKey(string key) =>
            string.Equals(key, TypesRootKey, System.StringComparison.Ordinal);

        private static bool IsTypeKey(string key) =>
            !string.IsNullOrWhiteSpace(key) && key.StartsWith("type:", System.StringComparison.OrdinalIgnoreCase);

        private static bool TryParseTypeKey(string key, out short cashTypeCode)
        {
            cashTypeCode = 0;
            if (!IsTypeKey(key)) return false;
            var s = key.Substring("type:".Length);
            return short.TryParse(s, out cashTypeCode);
        }

        private object BuildTypesRootNode()
        {
            return new
            {
                // Icon + label (escapeTitles is false, so HTML is allowed)
                title = "<i class='bi bi-list-ol me-1'></i> By Cash Type",
                key = TypesRootKey,
                folder = true,
                lazy = true,
                icon = false,
                data = new
                {
                    nodeType = "synthetic",
                    syntheticKind = "type",
                    isTypeContext = true,
                    canReorder = false
                }
            };
        }

        private async Task<List<object>> BuildTypeNodesAsync()
        {
            var types = await NodeContext.Cash_tbTypes
                .OrderBy(t => t.CashTypeCode)
                .Select(t => new { t.CashTypeCode, t.CashType })
                .ToListAsync();

            return types.Select(t => (object)new
            {
                title = $"<i class='bi bi-collection me-1'></i> {WebUtility.HtmlEncode(t.CashType)}",
                key = $"type:{t.CashTypeCode}",
                folder = true,
                lazy = true,
                icon = false,
                data = new
                {
                    nodeType = "synthetic",
                    syntheticKind = "type",
                    isTypeContext = true,
                    cashTypeCode = t.CashTypeCode,
                    canReorder = true
                }
            }).ToList();
        }

        // CategoryTypeCode == 0 (Cash Code), DisplayOrder: >0 first (asc), then 0 (uninitialized), stabilized by Category
        private async Task<List<object>> BuildCategoriesForTypeAsync(short cashTypeCode)
        {
            const short CATEGORYTYPE_CASHCODE = 0;

            var cats = await NodeContext.Cash_tbCategories
                .Where(c => c.CashTypeCode == cashTypeCode && c.CategoryTypeCode == CATEGORYTYPE_CASHCODE)
                .OrderBy(c => c.DisplayOrder == 0 ? 1 : 0)
                .ThenBy(c => c.DisplayOrder == 0 ? short.MaxValue : c.DisplayOrder)
                .ThenBy(c => c.Category)
                .Select(c => new
                {
                    c.CategoryCode,
                    c.Category,
                    c.DisplayOrder,
                    c.IsEnabled,
                    c.CategoryTypeCode,
                    c.CashPolarityCode
                })
                .ToListAsync();

            return cats.Select(c => (object)new
            {
                key = c.CategoryCode,
                title =
                    $"<span class='tc-cat-icon tc-cat-{PolarityClass(c.CashPolarityCode)}'></span> " +
                    $"{WebUtility.HtmlEncode(c.Category)} ({WebUtility.HtmlEncode(c.CategoryCode)})",
                folder = true,
                lazy = true,
                icon = false,
                extraClasses = c.IsEnabled == 0 ? "tc-disabled" : null,
                data = new
                {
                    nodeType = "category",
                    isEnabled = (int)c.IsEnabled,
                    displayOrder = (int)c.DisplayOrder,
                    categoryTypeCode = (int)c.CategoryTypeCode,
                    isTypeContext = true // mark categories as part of the Cash Type subtree
                }
            }).ToList();
        }
    }
}
