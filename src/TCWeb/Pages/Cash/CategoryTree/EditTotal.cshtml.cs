using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.AppServices.CategoryTree;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditTotalModel : DI_BasePageModel
    {
        private ICategoryTreeChangeControlService CategoryTreeService { get; }

        public EditTotalModel(NodeContext context,
            ICategoryTreeChangeControlService categoryTreeService) : base(context)
        {
            CategoryTreeService = categoryTreeService;
        }

        [BindProperty(SupportsGet = true)] public string VersionId { get; set; }

        [BindProperty] public string CategoryCode { get; set; }
        [BindProperty] public string Category { get; set; }
        [BindProperty] public bool IsEnabled { get; set; }
        [BindProperty] public string ParentKey { get; set; }
        [BindProperty] public string ReturnKey { get; set; }

        public bool OperationSucceeded { get; private set; }
        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(string key, bool embed = false, string returnKey = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    ErrorMessage = "Missing key.";
                    return embed ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.VersionId == VersionId && c.CategoryCode == key);

                if (cat == null)
                {
                    ErrorMessage = "Category not found.";
                    return embed ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                if (cat.CategoryTypeCode != (short)NodeEnum.CategoryType.CashTotal)
                {
                    ErrorMessage = "Not a Total category.";
                    return embed ? Content("<div class='text-danger small p-2'>Invalid category type</div>") : Page();
                }

                CategoryCode = cat.CategoryCode;
                Category = cat.Category;
                IsEnabled = cat.IsEnabled != 0;

                ParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == VersionId && t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                ReturnKey = string.IsNullOrWhiteSpace(returnKey) ? CategoryCode : returnKey;

                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = CategoryTreeCommandFeedback.From(ex);
                return embed
                    ? Content($"<div class='text-danger small p-2'>{System.Net.WebUtility.HtmlEncode(ErrorMessage)}</div>")
                    : Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(bool embed = false, string returnKey = null)
        {
            try
            {
                var isEmbedded =
                    embed
                    || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                    || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);

                // Prefer explicit ReturnKey from query/form, fallback to current category
                ReturnKey = !string.IsNullOrWhiteSpace(returnKey)
                    ? returnKey
                    : (Request.HasFormContentType ? Request.Form["ReturnKey"].ToString() : null);

                if (string.IsNullOrWhiteSpace(CategoryCode))
                {
                    ErrorMessage = "Missing key.";
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var cat = await NodeContext.Cash_tbCategories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.VersionId == VersionId
                        && c.CategoryCode == CategoryCode);

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

                await CategoryTreeService.SaveCategoryAsync(new CategoryTreeCategoryCommand(
                    VersionId, cat.CategoryCode,
                    string.IsNullOrWhiteSpace(Category) ? cat.Category : Category.Trim(),
                    cat.CategoryTypeCode, cat.CashPolarityCode, cat.CashTypeCode,
                    cat.DisplayOrder, IsEnabled ? (short)1 : (short)0,
                    "Total category edited."));

                OperationSucceeded = true;

                // Resolve parent for expand hint
                ParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == VersionId && t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                if (string.IsNullOrWhiteSpace(ReturnKey))
                {
                    ReturnKey = CategoryCode;
                }

                if (isEmbedded)
                {
                    return Page();
                }

                // Redirect with selection + expand so tree activates node and shows mobile footer
                return RedirectToPage("./Index", new {
                    versionId = VersionId,
                    select = ReturnKey,
                    expand = ParentKey
                });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = CategoryTreeCommandFeedback.From(ex);
                var isEmbedded =
                    embed
                    || (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal))
                    || string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal);
                return isEmbedded
                    ? Content($"<div class='text-danger small p-2'>{System.Net.WebUtility.HtmlEncode(ErrorMessage)}</div>")
                    : Page();
            }
        }
    }
}
