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

        // Already present in your code
        public string NewParentKey { get; private set; } = string.Empty;

        // New: used by client to anchor under type container (keys like "type:<CashTypeCode>")
        public string NewTypeKey { get; private set; } = string.Empty;

        // New: pipe-delimited chain of ancestors from top category (under type) down to TargetParentKey
        public string NewParentPath { get; private set; } = string.Empty;

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
                    .Select(x => new SelectListItem
                    {
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
                NewTypeKey = $"type:{tgt.CashTypeCode}";
                NewParentPath = await BuildAncestorPathAsync(TargetParentKey); // topCategory|...|TargetParentKey

                // Keep the old parent to discreetly collapse/update in UI
                ParentKey = OldParentKey;

                await OnGetAsync();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        // Build top-down chain of category codes from the top (under type) down to the given parent
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