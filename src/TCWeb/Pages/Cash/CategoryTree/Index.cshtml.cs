using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize]
    public partial class CategoryTreeModel : DI_BasePageModel
    {
        public const string DisconnectedNodeKey = "__DISCONNECTED__";
        public const string RootNodeKey = "__ROOT__";

        private ICategoryTreeChangeControlService CategoryTreeService { get; }
        private string WorkspaceVersionId { get; set; } = string.Empty;

        public CategoryTreeWorkspaceContext Workspace { get; private set; }

        [TempData]
        public string CategoryTreeNotice { get; set; }

        [TempData]
        public string CategoryTreeError { get; set; }

        public CategoryTreeModel(NodeContext context, ICategoryTreeChangeControlService categoryTreeService)
            : base(context)
        {
            CategoryTreeService = categoryTreeService;
        }

        public async Task OnGetAsync(string versionId = null)
        {
            Workspace = await CategoryTreeService.GetWorkspaceAsync(versionId, HttpContext.RequestAborted);
            WorkspaceVersionId = Workspace.SelectedVersion.VersionId;
            await SetViewData();
        }

        private static bool IsRootKey(string parentKey)
        {
            return string.IsNullOrEmpty(parentKey)
                || string.Equals(parentKey, RootNodeKey, StringComparison.Ordinal);
        }

        public Task<JsonResult> OnPostMoveUpAsync([FromForm] string versionId,
            [FromForm] string key, [FromForm] string parentKey)
        {
            return MoveVerticalAsync(versionId, key, parentKey, true);
        }

        public Task<JsonResult> OnPostMoveDownAsync([FromForm] string versionId,
            [FromForm] string key, [FromForm] string parentKey)
        {
            return MoveVerticalAsync(versionId, key, parentKey, false);
        }

        private async Task<JsonResult> MoveVerticalAsync(string versionId, string key,
            string parentKey, bool moveUp)
        {
            if (!User.IsInRole(Constants.AdministratorsRole))
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }

            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(key))
            {
                return new JsonResult(new { success = false, message = "Missing version or key" });
            }

            if (key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { success = false, message = "Codes are not reorderable" });
            }

            try
            {
                if (IsTypeParent(parentKey))
                {
                    return await ReorderCategoryByType(versionId, key, moveUp);
                }

                if (!await IsDisplayOrderInitialised(versionId, parentKey))
                {
                    await DisplayOrderInitialise(versionId, parentKey);
                }

                if (!IsRootKey(parentKey)
                    && !string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    var children = await NodeContext.Cash_tbCategoryTotals
                        .AsNoTracking()
                        .Where(item => item.VersionId == versionId && item.ParentCode == parentKey)
                        .OrderBy(item => item.DisplayOrder)
                        .ThenBy(item => item.ChildCode)
                        .Select(item => item.ChildCode)
                        .ToListAsync();
                    return await MoveInSequenceAsync(versionId, key, children, moveUp,
                        (orders) => SaveTotalOrdersAsync(versionId, parentKey, orders,
                            "Child categories reordered."));
                }

                var totals = await NodeContext.Cash_tbCategoryTotals
                    .AsNoTracking()
                    .Where(item => item.VersionId == versionId)
                    .ToListAsync();
                var linked = new HashSet<string>(totals
                    .SelectMany(item => new[] { item.ParentCode, item.ChildCode }),
                    StringComparer.Ordinal);
                var childrenSet = new HashSet<string>(totals.Select(item => item.ChildCode),
                    StringComparer.Ordinal);

                var categories = NodeContext.Cash_tbCategories
                    .AsNoTracking()
                    .Where(item => item.VersionId == versionId && item.IsEnabled != 0);
                categories = string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal)
                    ? categories.Where(item => !linked.Contains(item.CategoryCode))
                    : categories.Where(item => linked.Contains(item.CategoryCode)
                        && !childrenSet.Contains(item.CategoryCode));
                var sequence = await categories
                    .OrderBy(item => item.DisplayOrder)
                    .ThenBy(item => item.CategoryCode)
                    .Select(item => item.CategoryCode)
                    .ToListAsync();
                return await MoveInSequenceAsync(versionId, key, sequence, moveUp,
                    (orders) => SaveCategoryOrdersAsync(versionId, orders,
                        "Category order changed."));
            }
            catch (Exception exception)
            {
                await NodeContext.ErrorLog(exception);
                return new JsonResult(new { success = false, message = exception.Message });
            }
        }

        private static async Task<JsonResult> MoveInSequenceAsync(string versionId, string key,
            IList<string> sequence, bool moveUp,
            Func<IEnumerable<(string CategoryCode, short DisplayOrder)>, Task> save)
        {
            var index = sequence.IndexOf(key);
            var target = moveUp ? index - 1 : index + 1;
            if (index < 0)
            {
                return new JsonResult(new { success = false, message = "Node not found" });
            }
            if (target < 0 || target >= sequence.Count)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = moveUp ? "Already at top" : "Already at bottom"
                });
            }

            (sequence[index], sequence[target]) = (sequence[target], sequence[index]);
            await save(sequence.Select((code, order) => (code, (short)(order + 1))));
            return new JsonResult(new { success = true, versionId });
        }

        public async Task<JsonResult> OnPostReorderExpressionAsync([FromForm] string versionId,
            [FromForm] string key, [FromForm] string anchorKey, [FromForm] string mode)
        {
            if (!User.IsInRole(Constants.AdministratorsRole))
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }
            if (string.IsNullOrWhiteSpace(versionId) || string.IsNullOrWhiteSpace(key)
                || string.IsNullOrWhiteSpace(anchorKey) || (mode != "before" && mode != "after"))
            {
                return new JsonResult(new { success = false, message = "Invalid parameters" });
            }

            key = IsExpressionKey(key) ? key.Substring(ExpressionKeyPrefix.Length) : key;
            anchorKey = IsExpressionKey(anchorKey)
                ? anchorKey.Substring(ExpressionKeyPrefix.Length)
                : anchorKey;

            try
            {
                var sequence = await NodeContext.Cash_tbCategories
                    .AsNoTracking()
                    .Where(item => item.VersionId == versionId
                        && item.CategoryTypeCode == (short)NodeEnum.CategoryType.Expression)
                    .OrderBy(item => item.DisplayOrder)
                    .ThenBy(item => item.CategoryCode)
                    .Select(item => item.CategoryCode)
                    .ToListAsync();
                if (!sequence.Remove(key) || !sequence.Contains(anchorKey))
                {
                    return new JsonResult(new { success = false, message = "Expression not found" });
                }

                var index = sequence.IndexOf(anchorKey) + (mode == "after" ? 1 : 0);
                sequence.Insert(index, key);
                await SaveCategoryOrdersAsync(versionId,
                    sequence.Select((code, order) => (code, (short)(order + 1))),
                    "Expressions reordered.");
                return new JsonResult(new { success = true });
            }
            catch (Exception exception)
            {
                await NodeContext.ErrorLog(exception);
                return new JsonResult(new { success = false, message = exception.Message });
            }
        }
    }
}
