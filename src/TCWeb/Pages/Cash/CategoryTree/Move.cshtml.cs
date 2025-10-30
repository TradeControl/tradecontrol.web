using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models; // added

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class MoveModel : DI_BasePageModel
    {
        public MoveModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string Key { get; set; } = string.Empty; // CategoryCode to move

        [BindProperty(SupportsGet = true)]
        public string ParentKey { get; set; } = string.Empty; // current parent (if any) for default selection

        // View data
        public string CategoryName { get; private set; } = string.Empty;
        public string CategoryCode => Key;
        public short CashTypeCode { get; private set; }
        public short CashPolarityCode { get; private set; }

        // Target selection
        [BindProperty]
        public string TargetParentKey { get; set; } = string.Empty;
        public List<SelectListItem> CandidateParents { get; private set; } = new();

        // Result
        public bool OperationSucceeded { get; private set; }
        public string OldParentKey { get; private set; } = string.Empty;
        public string NewParentKey { get; private set; } = string.Empty; // <— add
        public string NewParentPath { get; private set; } = string.Empty; // pipe-delimited root->...->parent chain

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                {
                    return NotFound();
                }

                var candidates = await NodeContext.Cash_tbCategories
                    .Where(c => c.IsEnabled != 0
                                && c.CategoryTypeCode == (short)NodeEnum.CategoryType.CashTotal
                                && c.CategoryCode != Key)
                    .Select(c => new { c.CategoryCode, c.Category })
                    .OrderBy(c => c.Category)
                    .ToListAsync();

                CandidateParents = candidates
                    .Select(x => new SelectListItem
                    {
                        Value = x.CategoryCode,
                        Text = $"{x.Category} ({x.CategoryCode})"
                    })
                    .ToList();

                if (!string.IsNullOrWhiteSpace(ParentKey) && CandidateParents.Any(i => i.Value == ParentKey))
                {
                    TargetParentKey = ParentKey;
                }

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
                    .Select(c => new { c.CategoryCode, c.Category, c.CashTypeCode, c.CashPolarityCode })
                    .ToListAsync();

                var src = cats.FirstOrDefault(c => c.CategoryCode == Key);
                var tgt = cats.FirstOrDefault(c => c.CategoryCode == TargetParentKey);

                if (src == null || tgt == null)
                {
                    ModelState.AddModelError(string.Empty, "Category not found or disabled.");
                    await OnGetAsync();
                    return Page();
                }

                //if (src.CashTypeCode != tgt.CashTypeCode)
                //{
                //    ModelState.AddModelError(string.Empty, "Target must match the same cash type.");
                //    await OnGetAsync();
                //    return Page();
                //}

                // Let DB-side constraints/triggers handle polarity/cycles.

                OldParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == Key)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? string.Empty;

                using var tx = await NodeContext.Database.BeginTransactionAsync();

                await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == Key)
                    .ExecuteDeleteAsync();

                short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == TargetParentKey)
                    .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal
                {
                    ParentCode = TargetParentKey,
                    ChildCode = Key,
                    DisplayOrder = nextOrder
                });

                await NodeContext.SaveChangesAsync();
                await tx.CommitAsync();

                OperationSucceeded = true;

                // Preserve for client
                NewParentKey = TargetParentKey;
                ParentKey = OldParentKey;

                // Build full ancestor chain to parent (root -> ... -> parent)
                NewParentPath = await BuildAncestorPathAsync(TargetParentKey);

                // For full-page refresh (mobile), rebuild GET context
                await OnGetAsync();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private static HashSet<string> ComputeDescendants(string root, IEnumerable<dynamic> edges)
        {
            var map = edges
                .GroupBy(e => (string)e.ParentCode)
                .ToDictionary(g => g.Key, g => g.Select(x => (string)x.ChildCode).Where(s => !string.IsNullOrEmpty(s)).ToList());

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (!seen.Add(cur)) continue;
                if (map.TryGetValue(cur, out var children))
                {
                    foreach (var ch in children)
                        stack.Push(ch);
                }
            }

            seen.Remove(root);
            return seen;
        }

        private async Task<string> BuildAncestorPathAsync(string leafParent)
        {
            if (string.IsNullOrWhiteSpace(leafParent))
            {
                return string.Empty;
            }

            // child -> parent map
            var parentMap = await NodeContext.Cash_tbCategoryTotals
                .Where(t => t.ChildCode != null && t.ParentCode != null)
                .GroupBy(t => t.ChildCode)
                .Select(g => new { Child = g.Key, Parent = g.Select(x => x.ParentCode).FirstOrDefault() })
                .ToDictionaryAsync(x => x.Child, x => x.Parent);

            var chain = new List<string>();
            var cur = leafParent;
            var guard = 0;

            while (!string.IsNullOrEmpty(cur) && guard++ < 256)
            {
                chain.Add(cur);
                string p;
                if (!parentMap.TryGetValue(cur, out p) || string.IsNullOrEmpty(p))
                {
                    break;
                }
                cur = p;
            }

            chain.Reverse();
            // Use a safe delimiter for keys (pipe)
            return string.Join("|", chain);
        }
    }
}