using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    /// <summary>
    /// Build-only partial: provides the Nodes endpoint and builders.
    /// Includes disabled items and exposes nodeType/isEnabled to the UI.
    /// </summary>
    public partial class CategoryTreeModel
    {
        public async Task<JsonResult> OnGetNodesAsync(string id)
        {
            try
            {
                HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
                HttpContext.Response.Headers["Pragma"] = "no-cache";
                HttpContext.Response.Headers["Expires"] = "0";

                // Top-level anchors (empty id)
                if (string.IsNullOrEmpty(id))
                {
                    var totals = await NodeContext.Cash_tbCategoryTotals
                        .AsNoTracking()
                        .Select(t => new { t.ParentCode, t.ChildCode })
                        .ToListAsync();

                    var linkedSet = new HashSet<string>(
                        totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode })
                              .Where(s => !string.IsNullOrEmpty(s)));

                    bool disconnectedExist = await NodeContext.Cash_tbCategories
                        .AsNoTracking()
                        .AnyAsync(c => !linkedSet.Contains(c.CategoryCode));

                    var nodes = new List<object>
                    {
                        new {
                            key = RootNodeKey,
                            title = "<span class='tc-root-icon' style='font-weight:600;'>&#8721;</span> Root",
                            folder = true,
                            lazy = true,
                            icon = false,
                            data = new { isRoot = true, nodeType = "rootAnchor" }
                        }
                    };

                    if (disconnectedExist)
                    {
                        nodes.Add(new {
                            key = DisconnectedNodeKey,
                            title = "<i class='bi bi-plug tc-disconnected-icon'></i> Disconnected",
                            folder = true,
                            lazy = true,
                            icon = false,
                            data = new { disconnected = true, nodeType = "disconnectedAnchor" }
                        });
                    }

                    // Cash Types synthetic root
                    nodes.Add(BuildTypesRootNode());

                    // NEW: Cash Expressions synthetic root (single-level list)
                    nodes.Add(new {
                        key = ExpressionsNodeKey,
                        title = "<i class='bi bi-calculator'></i> Cash Expressions",
                        folder = true,
                        lazy = true,
                        icon = false,
                        data = new { syntheticKind = "expressionsRoot", nodeType = "expressionsRoot" }
                    });

                    return new JsonResult(nodes);
                }

                // Cash Type subtree
                if (IsTypesRootKey(id))
                {
                    var typeNodes = await BuildTypeNodesAsync();
                    return new JsonResult(typeNodes);
                }

                if (IsTypeKey(id) && TryParseTypeKey(id, out var typeCode))
                {
                    var catNodes = await BuildCategoriesForTypeAsync(typeCode);
                    return new JsonResult(catNodes);
                }

                // Cash Expressions subtree
                if (string.Equals(id, ExpressionsNodeKey, StringComparison.Ordinal))
                {
                    var exprNodes = await BuildExpressionNodesAsync();
                    return new JsonResult(exprNodes);
                }

                // Existing totals-based tree
                var (totals2, childCodesSet, linkedSet2) = await LoadTotalsAndSetsAsync();

                if (id == RootNodeKey)
                {
                    var nodes = await BuildRootNodesAsync(totals2, childCodesSet, linkedSet2);
                    return new JsonResult(nodes);
                }

                if (id == DisconnectedNodeKey)
                {
                    var nodes = await BuildDisconnectedNodesAsync(linkedSet2);
                    return new JsonResult(nodes);
                }

                var childNodes = await BuildChildNodesAsync(id, totals2);
                return new JsonResult(childNodes);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(Array.Empty<object>());
            }
        }

        private async Task<List<object>> BuildExpressionNodesAsync()
        {
            short exprType = (short)NodeEnum.CategoryType.Expression;

            var rows = await NodeContext.Cash_tbCategories
                .Where(c => c.CategoryTypeCode == exprType)
                .GroupJoin(
                    NodeContext.Cash_tbCategoryExps,
                    c => c.CategoryCode,
                    e => e.CategoryCode,
                    (c, expGroup) => new {
                        c.CategoryCode,
                        c.Category,
                        c.DisplayOrder,
                        c.CashTypeCode,
                        c.IsEnabled,
                        Exp = expGroup.FirstOrDefault()
                    }
                )
                // Order: non-zero DisplayOrder ascending, then zero (uninitialized), then CategoryCode for stability
                .OrderBy(r => r.DisplayOrder == 0)
                .ThenBy(r => r.DisplayOrder)
                .ThenBy(r => r.CategoryCode)
                .ToListAsync();

            return rows.Select(r =>
            {
                var expression = r.Exp?.Expression ?? "";
                var format = r.Exp?.Format ?? "";
                var isError = (r.Exp?.IsError ?? false) ? 1 : 0;

                string exprForTitle = expression;
                if (exprForTitle.Length > 60)
                    exprForTitle = exprForTitle.Substring(0, 57) + "...";

                var title =
                    "<span class='bi bi-calculator me-1'></span>"
                    + WebUtility.HtmlEncode(r.Category)
                    + " (" + WebUtility.HtmlEncode(r.CategoryCode) + ") "
                    + "<span class='tc-exp-formula'>= " + WebUtility.HtmlEncode(exprForTitle) + "</span>";

                return (object)new {
                    key = MakeExpressionKey(r.CategoryCode),
                    title = title,
                    folder = false,
                    lazy = false,
                    icon = false, // suppress default icon; we render our own in title
                    extraClasses = isError == 1 ? "tc-exp-error" : (r.IsEnabled == 0 ? "tc-disabled" : null),
                    data = new {
                        nodeType = "expression",
                        categoryCode = r.CategoryCode,
                        category = r.Category,
                        displayOrder = r.DisplayOrder,
                        cashTypeCode = r.CashTypeCode,
                        expression = expression,
                        format = format,
                        isError = isError,
                        isEnabled = r.IsEnabled == 0 ? 0 : 1
                    }
                };
            }).ToList();
        }

        private async Task<(List<Cash_tbCategoryTotal> totals, HashSet<string> childCodesSet, HashSet<string> linkedSet)> LoadTotalsAndSetsAsync()
        {
            var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
            var childCodesSet = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
            var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));
            return (totals, childCodesSet, linkedSet);
        }

        private async Task<List<object>> BuildRootNodesAsync(List<Cash_tbCategoryTotal> totals, HashSet<string> childCodesSet, HashSet<string> linkedSet)
        {
            var rootCandidates = await NodeContext.Cash_tbCategories
                                .Where(c => !childCodesSet.Contains(c.CategoryCode))
                                .ToListAsync();

            var cats = rootCandidates.Where(c => linkedSet.Contains(c.CategoryCode)).ToList();

            bool anyOrder = cats.Any(c => c.DisplayOrder > 0);
            cats = anyOrder
                ? cats.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Category).ToList()
                : cats.OrderBy(c => c.CashPolarityCode).ThenBy(c => c.Category).ToList();

            var rootNodes = cats.Select(c => (object)new {
                key = c.CategoryCode,
                title =
                    $"<span class='tc-cat-icon tc-cat-{PolarityClass(c.CashPolarityCode)}'></span> " +
                    $"{WebUtility.HtmlEncode(c.Category)} ({WebUtility.HtmlEncode(c.CategoryCode)})",
                folder = true,
                lazy = true,
                icon = false,
                extraClasses = c.IsEnabled == 0 ? "tc-disabled" : null,
                data = new {
                    cashPolarity = c.CashPolarityCode,
                    categoryType = c.CategoryTypeCode,
                    nodeType = "category",
                    isEnabled = c.IsEnabled == 0 ? 0 : 1
                }
            }).ToList();

            return rootNodes;
        }

        private async Task<List<object>> BuildDisconnectedNodesAsync(HashSet<string> linkedSet)
        {
            var disconnectedCats = await NodeContext.Cash_tbCategories
                .Where(c => !linkedSet.Contains(c.CategoryCode))
                .OrderBy(c => c.CashPolarityCode)
                .ThenBy(c => c.DisplayOrder)
                .ThenBy(c => c.Category)
                .ToListAsync();

            var discCatCodes = disconnectedCats.Select(c => c.CategoryCode).ToArray();
            var hasCodesSet = await NodeContext.Cash_tbCodes
                .Where(code => discCatCodes.Contains(code.CategoryCode))
                .GroupBy(code => code.CategoryCode)
                .Select(g => g.Key)
                .ToListAsync();
            var hasCodes = new HashSet<string>(hasCodesSet);

            var nodes = disconnectedCats.Select(c => (object)new {
                key = c.CategoryCode,
                title =
                    $"<span class='tc-cat-icon tc-cat-{PolarityClass(c.CashPolarityCode)}'></span> " +
                    $"{WebUtility.HtmlEncode(c.Category)} ({WebUtility.HtmlEncode(c.CategoryCode)})",
                folder = true,
                lazy = hasCodes.Contains(c.CategoryCode),
                icon = false,
                extraClasses = c.IsEnabled == 0 ? "tc-disabled" : null,
                data = new {
                    cashPolarity = c.CashPolarityCode,
                    categoryType = c.CategoryTypeCode,
                    nodeType = "category",
                    isEnabled = c.IsEnabled == 0 ? 0 : 1
                }
            }).ToList();

            return nodes;
        }

        private async Task<List<object>> BuildChildNodesAsync(string parentId, List<Cash_tbCategoryTotal> totals)
        {
            var childTotals = totals.Where(t => t.ParentCode == parentId).ToList();

            List<Cash_tbCategory> cats;
            if (childTotals.Count == 0)
            {
                cats = new List<Cash_tbCategory>();
            }
            else
            {
                var childCodes = childTotals.Select(t => t.ChildCode).ToArray();

                var dict = await NodeContext.Cash_tbCategories
                                .Where(c => childCodes.Contains(c.CategoryCode))
                                .ToDictionaryAsync(c => c.CategoryCode);

                bool anyOrder = childTotals.Any(t => t.DisplayOrder > 0);

                if (anyOrder)
                {
                    var orderedTotals = childTotals.OrderBy(t => t.DisplayOrder).ToList();
                    cats = orderedTotals
                        .Select(t => dict.TryGetValue(t.ChildCode, out var c) ? c : null)
                        .Where(c => c != null)
                        .ToList();
                }
                else
                {
                    cats = dict.Values
                        .OrderBy(c => c.CashPolarityCode)
                        .ThenBy(c => c.DisplayOrder)
                        .ThenBy(c => c.Category)
                        .ToList();
                }
            }

            short categoryPolarity = 2;
            short categoryCashType = 0;
            var parentCategory = await NodeContext.Cash_tbCategories
                                    .Where(c => c.CategoryCode == parentId)
                                    .Select(c => new { c.CashPolarityCode, c.CashTypeCode })
                                    .SingleOrDefaultAsync();
            if (parentCategory != null)
            {
                categoryPolarity = parentCategory.CashPolarityCode;
                categoryCashType = parentCategory.CashTypeCode;
            }

            var categoryCodes = cats.Select(c => c.CategoryCode).ToArray();
            var hasChildCategory = totals
                .Where(t => categoryCodes.Contains(t.ParentCode))
                .Select(t => t.ParentCode)
                .Distinct()
                .ToHashSet();

            var hasCodesSet = await NodeContext.Cash_tbCodes
                .Where(code => categoryCodes.Contains(code.CategoryCode))
                .GroupBy(code => code.CategoryCode)
                .Select(g => g.Key)
                .ToListAsync();
            var hasCodes = new HashSet<string>(hasCodesSet);

            object BuildCategoryNode(Cash_tbCategory c)
            {
                var title =
                    $"<span class='tc-cat-icon tc-cat-{PolarityClass(c.CashPolarityCode)}'></span> " +
                    $"{WebUtility.HtmlEncode(c.Category)} ({WebUtility.HtmlEncode(c.CategoryCode)})";

                return new {
                    key = c.CategoryCode,
                    title,
                    folder = true,
                    lazy = hasChildCategory.Contains(c.CategoryCode) || hasCodes.Contains(c.CategoryCode),
                    icon = false,
                    extraClasses = c.IsEnabled == 0 ? "tc-disabled" : null,
                    data = new {
                        cashPolarity = c.CashPolarityCode,
                        categoryType = c.CategoryTypeCode,
                        nodeType = "category",
                        isEnabled = c.IsEnabled == 0 ? 0 : 1
                    }
                };
            }

            object BuildCodeNode(dynamic cd)
            {
                var iconClass = CashCodeIconClass(categoryCashType);
                var title =
                    $"<span class='tc-code-icon bi {iconClass}'></span> " +
                    $"{WebUtility.HtmlEncode(cd.CashCode)} - {WebUtility.HtmlEncode(cd.CashDescription)}";

                return new {
                    key = $"code:{cd.CashCode}",
                    title,
                    folder = false,
                    lazy = false,
                    icon = false,
                    extraClasses = cd.IsEnabled == 0 ? "tc-disabled" : null,
                    data = new {
                        cashCode = cd.CashCode,
                        cashPolarity = categoryPolarity,
                        cashType = categoryCashType,
                        nodeType = "code",
                        isEnabled = cd.IsEnabled == 0 ? 0 : 1
                    }
                };
            }

            var categoryNodes = cats.Select(BuildCategoryNode).ToList();

            var codes = await NodeContext.Cash_tbCodes
                            .Where(code => code.CategoryCode == parentId)
                            .OrderBy(code => code.CashCode)
                            .Select(code => new { code.CashCode, code.CashDescription, code.IsEnabled })
                            .ToListAsync();

            var codeNodes = codes.Select(BuildCodeNode).ToList();

            return categoryNodes.Concat(codeNodes).ToList();
        }

        private static string PolarityClass(short polarityCode) =>
            polarityCode switch {
                0 => "expense",
                1 => "income",
                2 => "neutral",
                _ => "neutral"
            };

        private static string CashCodeIconClass(short cashTypeCode) =>
            cashTypeCode switch {
                1 => "bi-file-earmark-text",
                2 => "bi-bank",
                _ => "bi-wallet2"
            };
    }
}
