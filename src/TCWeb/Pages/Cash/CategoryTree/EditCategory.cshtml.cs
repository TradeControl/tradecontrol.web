using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditCategoryModel : DI_BasePageModel
    {
        public EditCategoryModel(NodeContext context) : base(context) { }
        [BindProperty] public string CategoryCode { get; set; } = "";
        [BindProperty] public string Category { get; set; } = "";
        [BindProperty] public bool IsEnabled { get; set; }
        [BindProperty] public string ParentKey { get; set; } = "";

        [BindProperty] public short CashTypeCode { get; set; }
        [BindProperty] public short CashPolarityCode { get; set; }

        public List<SelectListItem> CashTypes { get; private set; } = new();
        public List<SelectListItem> CashPolarities { get; private set; } = new();

        public bool OperationSucceeded { get; private set; }
        public string ErrorMessage { get; private set; } = "";

        public async Task<IActionResult> OnGetAsync(string key, bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    ErrorMessage = "Missing key.";
                    await BuildLists();
                    return embed ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == key);

                if (cat == null)
                {
                    ErrorMessage = "Category not found.";
                    await BuildLists();
                    return embed ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                if (cat.CategoryTypeCode != (short)NodeEnum.CategoryType.CashCode)
                {
                    ErrorMessage = "Not a Cash Code category.";
                    await BuildLists();
                    return embed ? Content("<div class='text-danger small p-2'>Invalid category type</div>") : Page();
                }

                CategoryCode = cat.CategoryCode;
                Category = cat.Category;
                IsEnabled = cat.IsEnabled != 0;
                CashTypeCode = cat.CashTypeCode;
                CashPolarityCode = cat.CashPolarityCode;

                ParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                await BuildLists();
                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                await BuildLists();
                return embed ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(bool embed = false)
        {
            var isEmbedded =
                embed
                || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);

            try
            {
                if (string.IsNullOrWhiteSpace(CategoryCode))
                {
                    ErrorMessage = "Missing key.";
                    await BuildLists();
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == CategoryCode);

                if (cat == null || cat.CategoryTypeCode != (short)NodeEnum.CategoryType.CashCode)
                {
                    ErrorMessage = "Category not found or invalid type.";
                    await BuildLists();
                    return Page();
                }

                if (!string.IsNullOrWhiteSpace(Category))
                {
                    cat.Category = Category.Trim();
                }
                cat.IsEnabled = IsEnabled ? (short)1 : (short)0;
                cat.CashTypeCode = CashTypeCode;
                cat.CashPolarityCode = CashPolarityCode;

                NodeContext.Attach(cat).State = EntityState.Modified;
                await NodeContext.SaveChangesAsync();

                ParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                OperationSucceeded = true;

                if (isEmbedded)
                {
                    return Page();
                }

                return RedirectToPage("./Index", new { select = CategoryCode, expand = ParentKey });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                await BuildLists();
                return isEmbedded ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }

        private async Task BuildLists()
        {
            CashTypes = await NodeContext.Cash_tbTypes
                .OrderBy(t => t.CashType)
                .Select(t => new SelectListItem { Value = t.CashTypeCode.ToString(), Text = t.CashType })
                .ToListAsync();

            CashPolarities = await NodeContext.Cash_tbPolaritys
                .OrderBy(p => p.CashPolarity)
                .Select(p => new SelectListItem { Value = p.CashPolarityCode.ToString(), Text = p.CashPolarity })
                .ToListAsync();
        }
    }
}
