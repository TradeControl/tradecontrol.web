using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public partial class CategoryTreeModel
    {
        private async Task SaveCategoryOrdersAsync(string versionId,
            IEnumerable<(string CategoryCode, short DisplayOrder)> orders, string changeNote)
        {
            var requested = orders.ToDictionary(item => item.CategoryCode,
                item => item.DisplayOrder, StringComparer.Ordinal);
            var categories = await NodeContext.Cash_tbCategories
                .AsNoTracking()
                .Where(item => item.VersionId == versionId && requested.Keys.Contains(item.CategoryCode))
                .ToListAsync(HttpContext.RequestAborted);

            if (categories.Count != requested.Count)
            {
                throw new InvalidOperationException("One or more Category Tree categories were not found in the selected version.");
            }

            var commands = categories.Select(item => new CategoryTreeCategoryCommand(
                versionId,
                item.CategoryCode,
                item.Category,
                item.CategoryTypeCode,
                item.CashPolarityCode,
                item.CashTypeCode,
                requested[item.CategoryCode],
                item.IsEnabled,
                changeNote)).ToArray();

            await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(
                versionId,
                commands,
                Array.Empty<CategoryTreeTotalCommand>(),
                Array.Empty<CategoryTreeCashCodeMoveCommand>()), HttpContext.RequestAborted);
        }

        private Task SaveTotalOrdersAsync(string versionId, string parentCode,
            IEnumerable<(string ChildCode, short DisplayOrder)> orders, string changeNote)
        {
            var commands = orders.Select(item => new CategoryTreeTotalCommand(
                versionId,
                parentCode,
                item.ChildCode,
                item.DisplayOrder,
                false,
                changeNote)).ToArray();

            return CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(
                versionId,
                Array.Empty<CategoryTreeCategoryCommand>(),
                commands,
                Array.Empty<CategoryTreeCashCodeMoveCommand>()), HttpContext.RequestAborted);
        }

        private Task ApplyTotalsAsync(string versionId, IEnumerable<CategoryTreeTotalCommand> totals)
        {
            return CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(
                versionId,
                Array.Empty<CategoryTreeCategoryCommand>(),
                totals.ToArray(),
                Array.Empty<CategoryTreeCashCodeMoveCommand>()), HttpContext.RequestAborted);
        }
    }
}
