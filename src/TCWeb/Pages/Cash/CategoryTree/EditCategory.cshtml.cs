using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Data;
namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditCategoryModel : DI_BasePageModel
    {
        private ICategoryTreeChangeControlService CategoryTreeService { get; }

        public EditCategoryModel(NodeContext context,
            ICategoryTreeChangeControlService categoryTreeService) : base(context)
        {
            CategoryTreeService = categoryTreeService;
        }

        [BindProperty] public string VersionId { get; set; } = "";
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

        public async Task<IActionResult> OnGetAsync(string key, string versionId = null,
            bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    ErrorMessage = "Missing key.";
                    await BuildLists();
                    return embed ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var workspace = await CategoryTreeService.GetWorkspaceAsync(versionId,
                    HttpContext.RequestAborted);
                VersionId = workspace.SelectedVersion.VersionId;

                var cat = await NodeContext.Cash_tbCategories.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.VersionId == VersionId && c.CategoryCode == key);

                if (cat == null)
                {
                    ErrorMessage = "Category not found.";
                    await BuildLists();
                    return embed ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                if (cat.CategoryTypeCode != (short)NodeEnum.CategoryType.Nominal)
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
                    .Where(t => t.VersionId == VersionId && t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                await BuildLists();
                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = CategoryTreeCommandFeedback.From(ex);
                await BuildLists();
                return embed
                    ? Content($"<div class='text-danger small p-2'>{System.Net.WebUtility.HtmlEncode(ErrorMessage)}</div>")
                    : Page();
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

                if (string.IsNullOrWhiteSpace(VersionId))
                {
                    ErrorMessage = "Missing Category Tree version context.";
                    await BuildLists();
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing version context</div>") : Page();
                }

                var cat = await NodeContext.Cash_tbCategories.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.VersionId == VersionId
                        && c.CategoryCode == CategoryCode);

                if (cat == null || cat.CategoryTypeCode != (short)NodeEnum.CategoryType.Nominal)
                {
                    ErrorMessage = "Category not found or invalid type.";
                    await BuildLists();
                    return Page();
                }

                await CategoryTreeService.SaveCategoryAsync(
                    new CategoryTreeCategoryCommand(
                        VersionId,
                        CategoryCode,
                        string.IsNullOrWhiteSpace(Category) ? cat.Category : Category.Trim(),
                        cat.CategoryTypeCode,
                        CashPolarityCode,
                        CashTypeCode,
                        cat.DisplayOrder,
                        IsEnabled ? (short)1 : (short)0,
                        $"Category {CategoryCode} edited."),
                    HttpContext.RequestAborted);

                ParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.VersionId == VersionId && t.ChildCode == CategoryCode)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync() ?? "";

                OperationSucceeded = true;

                if (isEmbedded)
                {
                    return Page();
                }

                return RedirectToPage("./Index", new
                {
                    versionId = VersionId,
                    select = CategoryCode,
                    expand = ParentKey
                });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = CategoryTreeCommandFeedback.From(ex);
                await BuildLists();
                return isEmbedded
                    ? Content($"<div class='text-danger small p-2'>{System.Net.WebUtility.HtmlEncode(ErrorMessage)}</div>")
                    : Page();
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
