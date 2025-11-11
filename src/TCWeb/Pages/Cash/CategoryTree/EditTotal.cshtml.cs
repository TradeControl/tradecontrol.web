using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditTotalModel : DI_BasePageModel
    {
        public EditTotalModel(NodeContext context) : base(context) { }

        [BindProperty]
        public string CategoryCode { get; set; }

        [BindProperty]
        public string Category { get; set; }

        [BindProperty]
        public bool IsEnabled { get; set; }

        [BindProperty]
        public string ParentKey { get; set; }

        public bool OperationSucceeded { get; private set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(string key, bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    ErrorMessage = "Missing key.";
                    return embed ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == key);

                if (cat == null)
                {
                    ErrorMessage = "Category not found.";
                    return embed ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                // Must be a Total category (CategoryType == CashTotal)
                if (cat.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Not a Total category.";
                    return embed ? Content("<div class='text-danger small p-2'>Invalid category type</div>") : Page();
                }

                CategoryCode = cat.CategoryCode;
                Category = cat.Category;
                IsEnabled = cat.IsEnabled != 0;

                // Determine parent (if any) via totals mapping
                ParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                return embed ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(bool embed = false)
        {
            try
            {
                // Robust embed detection (form OR query)
                var isEmbedded =
                    embed
                    || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                    || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);

                if (string.IsNullOrWhiteSpace(CategoryCode))
                {
                    ErrorMessage = "Missing key.";
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == CategoryCode);

                if (cat == null)
                {
                    ErrorMessage = "Category not found.";
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                if (cat.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Not a Total category.";
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Invalid category type</div>") : Page();
                }

                if (!string.IsNullOrWhiteSpace(Category))
                    cat.Category = Category.Trim();

                cat.IsEnabled = IsEnabled ? (short)1 : (short)0;

                NodeContext.Attach(cat).State = EntityState.Modified;
                await NodeContext.SaveChangesAsync();

                OperationSucceeded = true;

                // Re-evaluate ParentKey for the marker
                ParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                // Embedded: return the marker div (cshtml has Layout = null already)
                if (isEmbedded)
                    return Page();

                // Non-embedded: full redirect
                return RedirectToPage("./Index", new { key = CategoryCode });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                // Preserve embedded vs full response
                var isEmbedded =
                    embed
                    || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                    || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);

                return isEmbedded ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }
    }
}
