using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public class DeleteExpressionModel : DI_BasePageModel
    {
        public DeleteExpressionModel(NodeContext context) : base(context) { }

        [BindProperty]
        public string CategoryCode { get; set; } = string.Empty;

        [BindProperty]
        public string Category { get; set; } = string.Empty;

        [BindProperty]
        public string Expression { get; set; } = string.Empty;

        [BindProperty]
        public string Format { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return NotFound();
            }

            if (CategoryTreeModel.IsExpressionKey(key))
            {
                key = key.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);
            }

            var cat = await NodeContext.Cash_tbCategories
                .FirstOrDefaultAsync(c => c.CategoryCode == key);

            if (cat == null || cat.CategoryTypeCode != (short)NodeEnum.CategoryType.Expression)
            {
                return NotFound();
            }

            var exp = await NodeContext.Cash_tbCategoryExps
                .FirstOrDefaultAsync(e => e.CategoryCode == key);

            if (exp == null)
            {
                return NotFound();
            }

            CategoryCode = cat.CategoryCode;
            Category = cat.Category;
            Expression = exp.Expression;
            Format = exp.Format;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string key)
        {
            try
            {
                if (CategoryTreeModel.IsExpressionKey(key))
                {
                    key = key.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);
                }

                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == key);

                if (cat == null || cat.CategoryTypeCode != (short)NodeEnum.CategoryType.Expression)
                {
                    ModelState.AddModelError(string.Empty, "Expression category not found.");
                    return Page();
                }

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                NodeContext.Cash_tbCategories.Remove(cat);
                await NodeContext.SaveChangesAsync();

                await tx.CommitAsync();

                var exprRoot = CategoryTreeModel.ExpressionsNodeKey;
                var removedExprKey = CategoryTreeModel.MakeExpressionKey(key);

                if (Request.Query["embed"] == "1")
                {
                    ViewData["EmbedSelectKey"] = exprRoot;
                    ViewData["EmbedExpandKey"] = exprRoot;
                    ViewData["EmbedRemoveKey"] = removedExprKey;
                    return Page();
                }

                return RedirectToPage("/Cash/CategoryTree/Index", new {
                    select = exprRoot,
                    key = exprRoot,
                    expand = exprRoot,
                    remove = removedExprKey
                });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                throw;
            }
        }
    }
}
