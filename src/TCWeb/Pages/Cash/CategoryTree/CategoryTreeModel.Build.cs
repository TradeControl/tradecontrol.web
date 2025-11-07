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

                // Top-level: Root, Disconnected, and "By Cash Type"
                if (string.IsNullOrEmpty(id))
                {
                    // Determine if any disconnected categories exist
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
                    data = new { isRoot = true }
                }
            };

                    if (disconnectedExist)
                    {
                        nodes.Add(new
                        {
                            key = DisconnectedNodeKey,
                            title = "<i class='bi bi-plug tc-disconnected-icon'></i> Disconnected",
                            folder = true,
                            lazy = true,
                            icon = false,
                            data = new { disconnected = true }
                        });
                    }

                    nodes.Add(BuildTypesRootNode()); // Cash Types root

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

            var rootNodes = cats.Select(c => (object)new
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

            var nodes = disconnectedCats.Select(c => (object)new
            {
                key = c.CategoryCode,
                title =
                    $"<span class='tc-cat-icon tc-cat-{PolarityClass(c.CashPolarityCode)}'></span> " +
                    $"{WebUtility.HtmlEncode(c.Category)} ({WebUtility.HtmlEncode(c.CategoryCode)})",
                folder = true,                       // was: false
                lazy = hasCodes.Contains(c.CategoryCode),
                icon = false,
                extraClasses = c.IsEnabled == 0 ? "tc-disabled" : null,
                data = new
                {
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

            var categoryNodes = cats.Select(c => (object)new
            {
                key = c.CategoryCode,
                title =
                    $"<span class='tc-cat-icon tc-cat-{PolarityClass(c.CashPolarityCode)}'></span> " +
                    $"{WebUtility.HtmlEncode(c.Category)} ({WebUtility.HtmlEncode(c.CategoryCode)})",
                folder = true,
                lazy = hasChildCategory.Contains(c.CategoryCode) || hasCodes.Contains(c.CategoryCode),
                icon = false,
                extraClasses = c.IsEnabled == 0 ? "tc-disabled" : null,
                data = new
                {
                    cashPolarity = c.CashPolarityCode,
                    categoryType = c.CategoryTypeCode,
                    nodeType = "category",
                    isEnabled = c.IsEnabled == 0 ? 0 : 1
                }
            }).ToList();

            var codes = await NodeContext.Cash_tbCodes
                            .Where(code => code.CategoryCode == parentId)
                            .OrderBy(code => code.CashCode)
                            .Select(code => new { code.CashCode, code.CashDescription, code.IsEnabled })
                            .ToListAsync();

            // inside BuildChildNodesAsync where codeNodes is created — replace the codeNodes construction with:

            // Replace the existing codeNodes construction inside BuildChildNodesAsync with this block:

            var codeNodes = codes.Select(cd => (object)new {
                key = $"code:{cd.CashCode}",
                // Title without inline icon HTML; Fancytree will render the icon from `icon` property
                title = $"{WebUtility.HtmlEncode(cd.CashCode)} - {WebUtility.HtmlEncode(cd.CashDescription)}",
                folder = false,
                lazy = false,
                // Set icon to the bootstrap-icon class + helper class so only one icon is rendered
                icon = "bi " + CashCodeIconClass(categoryCashType) + " tc-code-icon",
                extraClasses = cd.IsEnabled == 0 ? "tc-disabled" : null,
                data = new {
                    cashCode = cd.CashCode,
                    cashPolarity = categoryPolarity,
                    cashType = categoryCashType,
                    nodeType = "code",
                    isEnabled = cd.IsEnabled == 0 ? 0 : 1
                }
            }).ToList();

            return categoryNodes.Concat(codeNodes).ToList();
        }

        private static string PolarityClass(short polarityCode)
        {
            return polarityCode switch
            {
                0 => "expense",
                1 => "income",
                2 => "neutral",
                _ => "neutral"
            };
        }

        private static string CashCodeIconClass(short cashTypeCode)
        {
            return cashTypeCode switch
            {
                1 => "bi-file-earmark-text",
                2 => "bi-bank",
                _ => "bi-wallet2"
            };
        }
    }
}
