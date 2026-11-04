using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class CreateCategoryModel : DI_BasePageModel
    {
        private ICategoryTreeChangeControlService CategoryTreeService { get; }

        public CreateCategoryModel(NodeContext context,
            ICategoryTreeChangeControlService categoryTreeService) : base(context)
        {
            CategoryTreeService = categoryTreeService;
        }

        [BindProperty(SupportsGet = true)]
        public string VersionId { get; set; } = "";

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

        public string NewNodeJson { get; private set; } = "";

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

            if (string.IsNullOrWhiteSpace(VersionId) || string.IsNullOrWhiteSpace(CategoryCode)
                || string.IsNullOrWhiteSpace(Category))
            {
                ModelState.AddModelError(string.Empty, "Category code and name are required.");
                return Page();
            }

            if (await NodeContext.Cash_tbCategories.AnyAsync(c => c.VersionId == VersionId
                && c.CategoryCode == CategoryCode))
            {
                ModelState.AddModelError(nameof(CategoryCode), "Category code already exists.");
                return Page();
            }

            try
            {
                var categoryCommand = new CategoryTreeCategoryCommand(VersionId, CategoryCode,
                    Category, (short)NodeEnum.CategoryType.Nominal, CashPolarityCode,
                    CashTypeCode, 0, IsEnabled ? (short)1 : (short)0, "Category created.");
                var totals = new System.Collections.Generic.List<CategoryTreeTotalCommand>();

                if (!string.IsNullOrWhiteSpace(ParentKey)
                    && !string.Equals(ParentKey, CategoryTreeModel.DisconnectedNodeKey, StringComparison.Ordinal))
                {
                    short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == VersionId && t.ParentCode == ParentKey)
                        .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                    totals.Add(new CategoryTreeTotalCommand(VersionId, ParentKey, CategoryCode,
                        nextOrder, false, "Category attached."));
                }
                await CategoryTreeService.ApplyBatchAsync(new CategoryTreeMutationBatchCommand(VersionId,
                    new[] { categoryCommand }, totals, Array.Empty<CategoryTreeCashCodeMoveCommand>()));

                OperationSucceeded = true;
                NewKey = CategoryCode;

                NewNodeJson = System.Text.Json.JsonSerializer.Serialize(new {
                    key = NewKey,            // category code
                    title = Category,        // display name
                    folder = true,
                    lazy = false,
                    data = new {
                        nodeType = "category",
                        categoryType = (short)NodeEnum.CategoryType.Nominal, // you set CategoryTypeCode earlier
                        cashPolarity = CashPolarityCode,
                        cashType = CashTypeCode,
                        isEnabled = IsEnabled ? 1 : 0
                    }
                });

                // Determine the best parent hint for selection/expand on mobile
                var parentHint = !string.IsNullOrWhiteSpace(ParentKey)
                    ? ParentKey
                    : $"type:{CashTypeCode}";

                // Embedded desktop (AJAX/embed=1) => return Page()
                var isEmbedded =
                    string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal)
                    || string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                    || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal));

                if (isEmbedded)
                    return Page();

                // Mobile/full-page: redirect with selection hints
                return RedirectToPage("/Cash/CategoryTree/Index",
                    new { versionId = VersionId, select = CategoryCode, parentKey = parentHint,
                        expand = parentHint });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ModelState.AddModelError(string.Empty, CategoryTreeCommandFeedback.From(ex));
                return Page();
            }
        }
    }
}
