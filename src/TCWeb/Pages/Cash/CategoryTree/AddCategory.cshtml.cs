using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class AddCategoryModel : DI_BasePageModel
    {
        private ICategoryTreeChangeControlService CategoryTreeService { get; }

        public AddCategoryModel(NodeContext context,
            ICategoryTreeChangeControlService categoryTreeService) : base(context)
        {
            CategoryTreeService = categoryTreeService;
        }

        [BindProperty(SupportsGet = true)]
        public string VersionId { get; set; } = string.Empty;

        [BindProperty]
        public string ParentKey { get; set; }

        [BindProperty]
        public string ChildKey { get; set; }

        public string ChildName { get; private set; }
        public short ChildPolarity { get; private set; }
        public bool ChildIsEnabled { get; private set; }

        public bool OperationSucceeded { get; private set; }
        public string ErrorMessage { get; private set; }

        public string Mode { get; private set; } = string.Empty;

        public List<SelectListItem> CategoryList { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(string parentKey, bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentKey))
                {
                    ErrorMessage = "Missing parent key.";
                    await PopulateOptionsAsync(null);
                    return embed
                        ? Content("<div class='text-danger small p-2'>Missing parent key</div>")
                        : Page();
                }

                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == VersionId && c.CategoryCode == parentKey)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode, c.IsEnabled })
                    .FirstOrDefaultAsync();

                if (parent == null || parent.IsEnabled == 0)
                {
                    ErrorMessage = "Parent not found or disabled.";
                    ParentKey = parentKey;
                    await PopulateOptionsAsync(null);
                    return embed
                        ? Content("<div class='text-danger small p-2'>Parent not found or disabled</div>")
                        : Page();
                }

                if (parent.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Parent category type is invalid for this operation.";
                    ParentKey = parentKey;
                    await PopulateOptionsAsync(null);
                    return embed
                        ? Content("<div class='text-danger small p-2'>Invalid parent type</div>")
                        : Page();
                }

                ParentKey = parentKey;
                await PopulateOptionsAsync(null);
                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = CategoryTreeCommandFeedback.From(ex);
                ParentKey = parentKey;
                await PopulateOptionsAsync(null);
                return embed
                    ? Content($"<div class='text-danger small p-2'>{System.Net.WebUtility.HtmlEncode(ErrorMessage)}</div>")
                    : Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(bool embed = false)
        {
            var isEmbedded =
                embed
                || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);

            var postedParent = Request.Form["ParentKey"].FirstOrDefault();
            var postedChild = Request.Form["ChildKey"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(postedParent))
                ParentKey = postedParent;

            if (!string.IsNullOrWhiteSpace(postedChild))
                ChildKey = postedChild;

            try
            {
                if (string.IsNullOrWhiteSpace(ParentKey))
                {
                    ErrorMessage = "Missing parent key.";
                    await PopulateOptionsAsync(ChildKey);
                    return isEmbedded
                        ? Content("<div class='text-danger small p-2'>Missing parent key</div>")
                        : Page();
                }

                if (string.IsNullOrWhiteSpace(ChildKey))
                {
                    ErrorMessage = "Select a Category to attach.";
                    await PopulateOptionsAsync(null);
                    return Page();
                }

                ChildKey = ChildKey.Trim();

                if (string.Equals(ParentKey, ChildKey, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Parent and child cannot be the same.";
                    await PopulateOptionsAsync(ChildKey);
                    return Page();
                }

                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == VersionId && c.CategoryCode == ParentKey)
                    .Select(c => new { c.CategoryCode, c.CategoryTypeCode, c.IsEnabled })
                    .FirstOrDefaultAsync();

                if (parent == null || parent.IsEnabled == 0)
                {
                    ErrorMessage = "Parent not found or disabled.";
                    await PopulateOptionsAsync(ChildKey);
                    return Page();
                }

                if (parent.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Parent category type is invalid for this operation.";
                    await PopulateOptionsAsync(ChildKey);
                    return Page();
                }

                var child = await NodeContext.Cash_tbCategories
                    .Where(c => c.VersionId == VersionId && c.CategoryCode == ChildKey)
                    .Select(c => new {
                        c.CategoryCode,
                        c.CategoryTypeCode,
                        c.Category,
                        c.CashPolarityCode,
                        c.IsEnabled
                    })
                    .FirstOrDefaultAsync();

                if (child == null || child.IsEnabled == 0)
                {
                    ErrorMessage = "Child not found or disabled.";
                    await PopulateOptionsAsync(ChildKey);
                    return Page();
                }

                if (child.CategoryTypeCode == (short)NodeEnum.CategoryType.Expression)
                {
                    ErrorMessage = "Child category type is invalid for this operation.";
                    await PopulateOptionsAsync(ChildKey);
                    return Page();
                }

                // Already attached: idempotent success
                var exists = await NodeContext.Cash_tbCategoryTotals
                    .AnyAsync(t => t.VersionId == VersionId
                        && t.ParentCode == ParentKey && t.ChildCode == ChildKey);

                // Decide add vs move for the Add action
                var childRoots = await GetRootAncestorsAsync(ChildKey);
                var targetRoots = await GetRootAncestorsAsync(ParentKey);
                bool sameTree = childRoots.Overlaps(targetRoots);

                Mode = sameTree ? "move" : "add";

                if (!exists)
                {
                    // Prevent cycles (ensure ParentKey is not a descendant of ChildKey)
                    var parentMap = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == VersionId
                            && t.ChildCode != null && t.ParentCode != null)
                        .GroupBy(t => t.ChildCode)
                        .Select(g => new {
                            Child = g.Key,
                            Parent = g.Select(x => x.ParentCode).FirstOrDefault()
                        })
                        .ToDictionaryAsync(x => x.Child, x => x.Parent);

                    bool cycleDetected = IsAncestor(ChildKey, ParentKey, parentMap);
                    if (cycleDetected)
                    {
                        ErrorMessage = "Operation would create a cycle.";
                        await PopulateOptionsAsync(ChildKey);
                        return Page();
                    }

                    var totals = new List<CategoryTreeTotalCommand>();
                    if (sameTree)
                    {
                        // Remove one existing edge for this child that belongs to the same tree as the target.
                        // (If none, this degenerates to a pure add.)
                        var existingParents = await NodeContext.Cash_tbCategoryTotals
                            .Where(t => t.VersionId == VersionId && t.ChildCode == ChildKey)
                            .Select(t => t.ParentCode)
                            .ToListAsync();

                        string oldParentInSameTree = null;
                        for (var i = 0; i < existingParents.Count; i++)
                        {
                            var p = existingParents[i];
                            if (string.IsNullOrWhiteSpace(p))
                                continue;

                            var pRoots = await GetRootAncestorsAsync(p);
                            if (pRoots.Overlaps(targetRoots))
                            {
                                oldParentInSameTree = p;
                                break;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(oldParentInSameTree))
                        {
                            totals.Add(new CategoryTreeTotalCommand(VersionId, oldParentInSameTree,
                                ChildKey, 0, true, "Category moved."));
                        }
                    }

                    // Normalize any zero display orders under new parent
                    var existingUnderNew = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == VersionId && t.ParentCode == ParentKey)
                        .OrderBy(t => t.DisplayOrder)
                        .ToListAsync();

                    if (existingUnderNew.Any(e => e.DisplayOrder == 0))
                    {
                        totals.AddRange(existingUnderNew.Select((row, index) =>
                            new CategoryTreeTotalCommand(VersionId, ParentKey, row.ChildCode,
                                (short)(index + 1), false, "Sibling order normalised.")));
                    }

                    short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == VersionId && t.ParentCode == ParentKey)
                        .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                    totals.Add(new CategoryTreeTotalCommand(VersionId, ParentKey, ChildKey,
                        nextOrder, false, "Category attached."));
                    await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(
                        VersionId, Array.Empty<CategoryTreeCategoryCommand>(), totals,
                        Array.Empty<CategoryTreeCashCodeMoveCommand>()));
                }

                ChildName = child.Category;
                ChildPolarity = child.CashPolarityCode;
                ChildIsEnabled = child.IsEnabled != 0;
                OperationSucceeded = true;

                if (isEmbedded)
                    return Page();

                return RedirectToPage("/Cash/CategoryTree/Index",
                    new { versionId = VersionId, select = ChildKey, parentKey = ParentKey,
                        expand = ParentKey });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = CategoryTreeCommandFeedback.From(ex);
                await PopulateOptionsAsync(ChildKey);
                return isEmbedded
                    ? Content($"<div class='text-danger small p-2'>{System.Net.WebUtility.HtmlEncode(ErrorMessage)}</div>")
                    : Page();
            }
        }

        private async Task<HashSet<string>> GetRootAncestorsAsync(string key)
        {
            var edges = await NodeContext.Cash_tbCategoryTotals
                .AsNoTracking()
                .Where(t => t.VersionId == VersionId && t.ParentCode != null && t.ChildCode != null)
                .Select(t => new { t.ParentCode, t.ChildCode })
                .ToListAsync();

            var parentsByChild = edges
                .GroupBy(e => e.ChildCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ParentCode)
                          .Where(p => !string.IsNullOrWhiteSpace(p))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList(),
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
                    roots.Add(cur);
                    continue;
                }

                foreach (var p in parents)
                    stack.Push(p);
            }

            return roots;
        }

        private async Task PopulateOptionsAsync(string selectedChildCode)
        {
            var attached = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(ParentKey))
            {
                var attachedList = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == VersionId && t.ParentCode == ParentKey)
                    .Select(t => t.ChildCode)
                    .ToListAsync();
                attached = new HashSet<string>(attachedList, StringComparer.OrdinalIgnoreCase);
            }

            short totalType = (short)NodeEnum.CategoryType.CashTotal;
            short expressionType = (short)NodeEnum.CategoryType.Expression;

            var rows = await (from c in NodeContext.Cash_tbCategories
                              join ct in NodeContext.Cash_tbTypes on c.CashTypeCode equals ct.CashTypeCode
                              where c.VersionId == VersionId && c.IsEnabled != 0
                                 && c.CategoryTypeCode != expressionType
                              select new {
                                  c.CategoryCode,
                                  c.Category,
                                  c.CashPolarityCode,
                                  c.CategoryTypeCode,
                                  CashType = ct.CashType
                              }).ToListAsync();

            rows = rows
                .Where(r => !string.Equals(r.CategoryCode, ParentKey, StringComparison.OrdinalIgnoreCase))
                .Where(r => !attached.Contains(r.CategoryCode))
                .ToList();

            var ordered = rows
                .OrderByDescending(r => r.CategoryTypeCode == totalType)
                .ThenBy(r => r.CashPolarityCode)
                .ThenBy(r => r.Category, StringComparer.OrdinalIgnoreCase)
                .ToList();

            CategoryList = ordered
                .Select(r => {
                    var text = $"{r.Category} ({r.CategoryCode})";
                    return new SelectListItem {
                        Value = r.CategoryCode,
                        Text = text,
                        Selected = !string.IsNullOrWhiteSpace(selectedChildCode)
                                   && string.Equals(selectedChildCode, r.CategoryCode, StringComparison.OrdinalIgnoreCase)
                    };
                })
                .ToList();
        }

        private static bool IsAncestor(string possibleAncestor, string startChild, IDictionary<string, string> parentMap)
        {
            var cur = startChild;
            var guard = 0;
            while (!string.IsNullOrEmpty(cur) && guard++ < 1024)
            {
                if (string.Equals(cur, possibleAncestor, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!parentMap.TryGetValue(cur, out var p) || string.IsNullOrEmpty(p))
                    break;

                cur = p;
            }
            return false;
        }
    }
}
