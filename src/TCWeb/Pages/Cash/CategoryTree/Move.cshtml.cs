using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class MoveModel : DI_BasePageModel
    {
        public MoveModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string Key { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ParentKey { get; set; } = string.Empty;

        public string CategoryName { get; private set; } = string.Empty;
        public string CategoryCode => Key;
        public short CashTypeCode { get; private set; }
        public short CashPolarityCode { get; private set; }

        [BindProperty]
        public string TargetParentKey { get; set; } = string.Empty;
        public List<SelectListItem> CandidateParents { get; private set; } = new();

        public bool OperationSucceeded { get; private set; }
        public string OldParentKey { get; private set; } = string.Empty;

        public string NewParentKey { get; private set; } = string.Empty;
        public string NewTypeKey { get; private set; } = string.Empty;
        public string NewParentPath { get; private set; } = string.Empty;

        public string Mode { get; private set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                    return NotFound();

                var candidates = await NodeContext.Cash_tbCategories
                    .Where(c => c.IsEnabled != 0
                                && c.CategoryTypeCode == (short)NodeEnum.CategoryType.CashTotal
                                && c.CategoryCode != Key)
                    .Select(c => new { c.CategoryCode, c.Category })
                    .OrderBy(c => c.Category)
                    .ToListAsync();

                CandidateParents = candidates
                    .Select(x => new SelectListItem {
                        Value = x.CategoryCode,
                        Text = $"{x.Category} ({x.CategoryCode})"
                    })
                    .ToList();

                if (!string.IsNullOrWhiteSpace(ParentKey) && CandidateParents.Any(i => i.Value == ParentKey))
                    TargetParentKey = ParentKey;

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await OnGetAsync();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(TargetParentKey))
                {
                    ModelState.AddModelError(string.Empty, "Missing parameters.");
                    await OnGetAsync();
                    return Page();
                }

                var cats = await NodeContext.Cash_tbCategories
                    .Where(c => (c.CategoryCode == Key || c.CategoryCode == TargetParentKey) && c.IsEnabled != 0)
                    .Select(c => new { c.CategoryCode, c.Category, c.CashTypeCode, c.CashPolarityCode, c.CategoryTypeCode })
                    .ToListAsync();

                var src = cats.FirstOrDefault(c => c.CategoryCode == Key);
                var tgt = cats.FirstOrDefault(c => c.CategoryCode == TargetParentKey);

                if (src == null || tgt == null)
                {
                    ModelState.AddModelError(string.Empty, "Category not found or disabled.");
                    await OnGetAsync();
                    return Page();
                }

                if (tgt.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ModelState.AddModelError(string.Empty, "Invalid target parent. Only Total-type categories may have child categories.");
                    await OnGetAsync();
                    return Page();
                }

                var alreadyAttached = await NodeContext.Cash_tbCategoryTotals
                    .AnyAsync(t => t.ParentCode == TargetParentKey && t.ChildCode == Key);

                if (alreadyAttached)
                {
                    OperationSucceeded = true;
                    OldParentKey = string.Empty;
                    NewParentKey = TargetParentKey;
                    NewTypeKey = $"type:{tgt.CashTypeCode}";
                    NewParentPath = await BuildAncestorPathAsync(TargetParentKey);
                    Mode = "add";
                    ParentKey = string.Empty;

                    return await ReturnEmbeddedOrRedirectAsync();
                }

                var keyRoots = await GetRootAncestorsAsync(Key);
                var targetRoots = await GetRootAncestorsAsync(TargetParentKey);
                var sameTree = keyRoots.Overlaps(targetRoots);

                Mode = sameTree ? "move" : "add";

                // For UX: prefer the existing parent in the same tree (when moving),
                // otherwise don't report an old parent.
                OldParentKey = string.Empty;
                if (sameTree)
                {
                    OldParentKey = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ChildCode == Key)
                        .OrderBy(t => t.ParentCode)
                        .Select(t => t.ParentCode)
                        .FirstOrDefaultAsync() ?? string.Empty;
                }

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                if (sameTree && !string.IsNullOrWhiteSpace(OldParentKey))
                {
                    await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == OldParentKey && t.ChildCode == Key)
                        .ExecuteDeleteAsync();
                }

                short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == TargetParentKey)
                    .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal {
                    ParentCode = TargetParentKey,
                    ChildCode = Key,
                    DisplayOrder = nextOrder
                });

                await NodeContext.SaveChangesAsync();
                await tx.CommitAsync();

                OperationSucceeded = true;

                NewParentKey = TargetParentKey;
                NewTypeKey = $"type:{tgt.CashTypeCode}";
                NewParentPath = await BuildAncestorPathAsync(TargetParentKey);

                // Preserve original semantics: ParentKey in query string should remain the old parent (when moving)
                ParentKey = OldParentKey;

                return await ReturnEmbeddedOrRedirectAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task<HashSet<string>> GetRootAncestorsAsync(string key)
        {
            var edges = await NodeContext.Cash_tbCategoryTotals
                .AsNoTracking()
                .Where(t => t.ParentCode != null && t.ChildCode != null)
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

        private async Task<IActionResult> ReturnEmbeddedOrRedirectAsync()
        {
            bool isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (isAjax || Request.Query["embed"] == "1")
            {
                var enc = HtmlEncoder.Default;
                var html =
                    "<div id=\"moveResult\""
                    + " data-key=\"" + enc.Encode(Key) + "\""
                    + " data-parent=\"" + enc.Encode(NewParentKey ?? string.Empty) + "\""
                    + " data-old=\"" + enc.Encode(OldParentKey ?? string.Empty) + "\""
                    + " data-type=\"" + enc.Encode(NewTypeKey ?? string.Empty) + "\""
                    + " data-path=\"" + enc.Encode(NewParentPath ?? string.Empty) + "\""
                    + " data-mode=\"" + enc.Encode(Mode ?? string.Empty) + "\"></div>";

                return Content(html, "text/html");
            }

            return RedirectToPage("/Cash/CategoryTree/Index", new { select = Key, parentKey = TargetParentKey });
        }

        private async Task<string> BuildAncestorPathAsync(string leafParent)
        {
            if (string.IsNullOrWhiteSpace(leafParent))
                return string.Empty;

            var parentMap = await NodeContext.Cash_tbCategoryTotals
                .Where(t => t.ChildCode != null && t.ParentCode != null)
                .GroupBy(t => t.ChildCode)
                .Select(g => new { Child = g.Key, Parent = g.Select(x => x.ParentCode).FirstOrDefault() })
                .ToDictionaryAsync(x => x.Child, x => x.Parent);

            var chain = new List<string>();
            var cur = leafParent;
            var guard = 0;

            while (!string.IsNullOrEmpty(cur) && guard++ < 512)
            {
                chain.Add(cur);
                if (!parentMap.TryGetValue(cur, out var p) || string.IsNullOrEmpty(p))
                    break;
                cur = p;
            }

            chain.Reverse();
            return string.Join("|", chain);
        }
    }
}
