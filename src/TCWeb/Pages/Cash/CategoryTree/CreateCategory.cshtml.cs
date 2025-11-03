using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class CreateCategoryModel : DI_BasePageModel
    {
        public CreateCategoryModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string ParentKey { get; set; } = "";

        [BindProperty]
        public string CategoryCode { get; set; } = "";

        [BindProperty]
        public string Category { get; set; } = "";

        [BindProperty]
        public short CashTypeCode { get; set; }

        [BindProperty]
        public short CashPolarityCode { get; set; }

        [BindProperty]
        public bool IsEnabled { get; set; } = true;

        public SelectList CashTypes { get; private set; }
        public SelectList Polarities { get; private set; }

        public bool OperationSucceeded { get; private set; }
        public string NewKey { get; private set; } = "";
        public string NewParentKey { get; private set; } = "";

        // Populate select lists only (do not overwrite bound properties)
        private async Task PopulateSelectListsAsync()
        {
            var types = await NodeContext.Cash_tbTypes
                .OrderBy(t => t.CashType)
                .Select(t => new { t.CashTypeCode, t.CashType })
                .ToListAsync();
            CashTypes = new SelectList(types, "CashTypeCode", "CashType");

            var pols = await NodeContext.Cash_tbPolaritys
                .OrderBy(p => p.CashPolarity)
                .Select(p => new { p.CashPolarityCode, p.CashPolarity })
                .ToListAsync();
            Polarities = new SelectList(pols, "CashPolarityCode", "CashPolarity");
        }

        public async Task OnGetAsync()
        {
            await PopulateSelectListsAsync();

            // sensible defaults for initial GET only
            CashTypeCode = (short)NodeEnum.CashType.Trade;
            CashPolarityCode = (short)NodeEnum.CashPolarity.Neutral;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Populate lists for re-rendering the form on validation failure,
            // but do NOT overwrite bound properties (CashPolarityCode, CashTypeCode).
            await PopulateSelectListsAsync();

            if (string.IsNullOrWhiteSpace(CategoryCode) || string.IsNullOrWhiteSpace(Category))
            {
                ModelState.AddModelError(string.Empty, "Category code and name are required.");
                return Page();
            }

            if (await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == CategoryCode))
            {
                ModelState.AddModelError(nameof(CategoryCode), "Category code already exists.");
                return Page();
            }

            try
            {
                var cat = new Cash_tbCategory
                {
                    CategoryCode = CategoryCode,
                    Category = Category,
                    // This category represents a Cash Code (affects entries/behaviour)
                    CategoryTypeCode = (short)NodeEnum.CategoryType.CashCode,
                    CashTypeCode = CashTypeCode,
                    CashPolarityCode = CashPolarityCode,
                    DisplayOrder = 0,
                    IsEnabled = IsEnabled ? (short)1 : (short)0
                };

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                NodeContext.Cash_tbCategories.Add(cat);
                await NodeContext.SaveChangesAsync();

                // If parentKey is provided and not the special disconnected key, attach under parent.
                if (!string.IsNullOrWhiteSpace(ParentKey)
                    && !string.Equals(ParentKey, TradeControl.Web.Pages.Cash.CategoryCode.CategoryTreeModel.DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.ParentCode == ParentKey)
                        .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                    NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal
                    {
                        ParentCode = ParentKey,
                        ChildCode = CategoryCode,
                        DisplayOrder = nextOrder
                    });

                    await NodeContext.SaveChangesAsync();
                }

                await tx.CommitAsync();

                OperationSucceeded = true;
                NewKey = CategoryCode;
                // If a parentKey was supplied use that; otherwise, for CashCode categories
                // expose the synthetic type parent so the client can reload only the proper type subtree.
                if (!string.IsNullOrWhiteSpace(ParentKey))
                {
                    NewParentKey = ParentKey;
                }
                else
                {
                    // For CashCode categories the tree places them under "type:<CashTypeCode>"
                    NewParentKey = $"type:{cat.CashTypeCode}";
                }

                if (Request.Query["embed"] == "1")
                    return Page();

                return RedirectToPage("/Cash/CategoryTree/Index");
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ModelState.AddModelError(string.Empty, "Server error.");
                return Page();
            }
        }
    }
}