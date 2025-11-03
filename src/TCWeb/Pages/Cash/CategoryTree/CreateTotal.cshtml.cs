using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class CreateTotalModel : DI_BasePageModel
    {
        public CreateTotalModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string ParentKey { get; set; } = "";

        [BindProperty]
        public string CategoryCode { get; set; } = "";

        [BindProperty]
        public string Category { get; set; } = "";

        // Hard-coded: Totals are CategoryType == CashTotal, CashType == Trade, Polarity == Neutral
        public short CashTypeCode { get; private set; } = (short)NodeEnum.CashType.Trade;
        public short CashPolarityCode { get; private set; } = (short)NodeEnum.CashPolarity.Neutral;

        [BindProperty]
        public bool IsEnabled { get; set; } = true;

        public bool OperationSucceeded { get; private set; }
        public string NewKey { get; private set; } = "";
        public string NewParentKey { get; private set; } = "";

        // New marker metadata
        public string NewName { get; private set; } = "";
        public short NewPolarity { get; private set; }
        public short NewCategoryType { get; private set; }
        public short NewIsEnabled { get; private set; }

        public Task OnGetAsync()
        {
            // Values are fixed for Totals - no DB calls required here.
            CashTypeCode = (short)NodeEnum.CashType.Trade;
            CashPolarityCode = (short)NodeEnum.CashPolarity.Neutral;
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await OnGetAsync();

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
                    CategoryTypeCode = (short)NodeEnum.CategoryType.CashTotal,
                    CashTypeCode = CashTypeCode,
                    CashPolarityCode = CashPolarityCode,
                    DisplayOrder = 0,
                    IsEnabled = IsEnabled ? (short)1 : (short)0
                };

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                NodeContext.Cash_tbCategories.Add(cat);
                await NodeContext.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(ParentKey))
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
                NewParentKey = ParentKey ?? "";

                // Populate marker metadata
                NewName = cat.Category;
                NewPolarity = cat.CashPolarityCode;
                NewCategoryType = cat.CategoryTypeCode;
                NewIsEnabled = cat.IsEnabled;

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