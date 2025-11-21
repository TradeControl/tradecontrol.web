// PATCH: remove unnecessary async from GET handler to clear CS1998 warning.
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

        public short CashTypeCode { get; private set; } = (short)NodeEnum.CashType.Trade;
        public short CashPolarityCode { get; private set; } = (short)NodeEnum.CashPolarity.Neutral;

        [BindProperty]
        public bool IsEnabled { get; set; } = true;

        public bool OperationSucceeded { get; private set; }
        public string NewKey { get; private set; } = "";
        public string NewParentKey { get; private set; } = "";

        public string NewName { get; private set; } = "";
        public short NewPolarity { get; private set; }
        public short NewCategoryType { get; private set; }
        public short NewIsEnabled { get; private set; }

        // Synchronous now: no awaited work inside.
        public IActionResult OnGet()
        {
            SetFixedValues();

            if (string.IsNullOrWhiteSpace(ParentKey))
            {
                try
                {
                    var q = HttpContext?.Request?.Query["parentKey"].ToString();
                    if (!string.IsNullOrWhiteSpace(q))
                        ParentKey = q;
                }
                catch { }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            SetFixedValues();

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

            if (string.IsNullOrWhiteSpace(ParentKey))
            {
                ModelState.AddModelError(nameof(ParentKey), "Parent category is required.");
                return Page();
            }

            try
            {
                var cat = new Cash_tbCategory {
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
                    var parentIsRealCategory = await NodeContext.Cash_tbCategories
                        .AnyAsync(c => c.CategoryCode == ParentKey);

                    if (parentIsRealCategory)
                    {
                        short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                            .Where(t => t.ParentCode == ParentKey)
                            .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                        NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal {
                            ParentCode = ParentKey,
                            ChildCode = CategoryCode,
                            DisplayOrder = nextOrder
                        });

                        await NodeContext.SaveChangesAsync();
                    }
                }

                await tx.CommitAsync();

                OperationSucceeded = true;
                NewKey = CategoryCode;
                NewParentKey = ParentKey ?? "";
                NewName = cat.Category;
                NewPolarity = cat.CashPolarityCode;
                NewCategoryType = cat.CategoryTypeCode;
                NewIsEnabled = cat.IsEnabled;

                bool isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
                if (isAjax && Request.Query["embed"] == "1")
                    return Page();

                return RedirectToPage("/Cash/CategoryTree/Index", new { key = NewKey, parentKey = NewParentKey });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ModelState.AddModelError(string.Empty, "Server error.");
                return Page();
            }
        }

        private void SetFixedValues()
        {
            CashTypeCode = (short)NodeEnum.CashType.Trade;
            CashPolarityCode = (short)NodeEnum.CashPolarity.Neutral;
        }
    }
}
