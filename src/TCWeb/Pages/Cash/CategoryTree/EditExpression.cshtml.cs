#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public class EditExpressionModel : DI_BasePageModel
    {
        public EditExpressionModel(NodeContext context) : base(context) { }

        // --- Bound fields ---
        [BindProperty, Required, StringLength(10), Display(Name = "Category Code")]
        public string CategoryCode { get; set; } = string.Empty;

        [BindProperty, Required, StringLength(50), Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [BindProperty, Required, Display(Name = "Cash Type")]
        public short? CashTypeCode { get; set; }

        [BindProperty, Required, StringLength(256), Display(Name = "Expression")]
        public string Expression { get; set; } = string.Empty;

        [BindProperty, Required, StringLength(100), Display(Name = "Format")]
        public string Format { get; set; } = string.Empty;

        // NEW: Syntax type selection
        [BindProperty, Required, Display(Name = "Syntax Type")]
        public short? SyntaxTypeCode { get; set; }

        // Lookup lists
        public IEnumerable<SelectListItem> CashTypeItems { get; private set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> SyntaxTypeItems { get; private set; } = Enumerable.Empty<SelectListItem>();
        public IList<string> ExistingFormats { get; private set; } = new List<string>();

        // Evaluation state
        public bool IsError { get; private set; }
        public string? ErrorMessage { get; private set; }

        public bool SaveSucceeded { get; private set; }

        public async Task<IActionResult> OnGetAsync(string key)
        {
            var catCode = NormalizeCategoryCode(key);

            var cat = await NodeContext.Cash_tbCategories
                .Where(c => c.CategoryCode == catCode && c.CategoryTypeCode == (short)NodeEnum.CategoryType.Expression)
                .Select(c => new { c.CategoryCode, c.Category, c.CashTypeCode, c.DisplayOrder, c.IsEnabled })
                .SingleOrDefaultAsync();

            if (cat == null)
                return NotFound();

            var exp = await NodeContext.Cash_tbCategoryExps
                .Where(e => e.CategoryCode == cat.CategoryCode)
                .Include(e => e.SyntaxTypeCodeNavigation)
                .SingleOrDefaultAsync();

            CategoryCode = cat.CategoryCode;
            Category = cat.Category;
            CashTypeCode = cat.CashTypeCode;
            Expression = exp?.Expression ?? string.Empty;
            Format = exp?.Format ?? string.Empty;
            SyntaxTypeCode = exp?.SyntaxTypeCode ?? 0; // default 'Both'
            IsError = exp?.IsError ?? false;
            ErrorMessage = string.IsNullOrWhiteSpace(exp?.ErrorMessage) ? null : exp!.ErrorMessage;

            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string key)
        {
            try
            {
                await LoadLookupsAsync();

                if (!ModelState.IsValid)
                    return Page();

                var catCode = NormalizeCategoryCode(key ?? CategoryCode);

                var cat = await NodeContext.Cash_tbCategories
                    .SingleOrDefaultAsync(c => c.CategoryCode == catCode && c.CategoryTypeCode == (short)NodeEnum.CategoryType.Expression);

                if (cat == null)
                {
                    ModelState.AddModelError(string.Empty, "Expression category not found.");
                    return Page();
                }

                var exp = await NodeContext.Cash_tbCategoryExps
                    .SingleOrDefaultAsync(e => e.CategoryCode == catCode);

                if (exp == null)
                {
                    exp = new Cash_tbCategoryExp { CategoryCode = catCode, IsError = false, ErrorMessage = null };
                    NodeContext.Cash_tbCategoryExps.Add(exp);
                }

                cat.Category = Category;
                cat.CashTypeCode = CashTypeCode ?? cat.CashTypeCode;

                exp.Expression = Expression;
                exp.Format = Format;
                exp.SyntaxTypeCode = SyntaxTypeCode ?? 0;

                IsError = exp.IsError;
                ErrorMessage = string.IsNullOrWhiteSpace(exp.ErrorMessage) ? null : exp.ErrorMessage;

                await NodeContext.SaveChangesAsync();
                SaveSucceeded = true;

                var exprKey = CategoryTreeModel.MakeExpressionKey(catCode);

                if (Request.Query["embed"] == "1")
                {
                    return RedirectToPage("/Cash/CategoryTree/Details", new { key = exprKey, embed = "1" });
                }

                return RedirectToPage("/Cash/CategoryTree/Index", new {
                    select = exprKey,
                    key = exprKey,
                    expand = CategoryTreeModel.ExpressionsNodeKey
                });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                throw;
            }
        }

        private string NormalizeCategoryCode(string keyOrCode)
        {
            if (string.IsNullOrWhiteSpace(keyOrCode))
                return string.Empty;
            var k = keyOrCode.Trim();
            if (CategoryTreeModel.IsExpressionKey(k))
                return k.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);
            return k;
        }

        private async Task LoadLookupsAsync()
        {
            var types = await NodeContext.Cash_tbTypes
                .OrderBy(t => t.CashType)
                .Select(t => new { t.CashTypeCode, t.CashType })
                .ToListAsync();

            CashTypeItems = types.Select(t => new SelectListItem {
                Value = t.CashTypeCode.ToString(),
                Text = $"{t.CashType} ({t.CashTypeCode})",
                Selected = CashTypeCode.HasValue && CashTypeCode.Value == t.CashTypeCode
            }).ToList();

            var syntaxes = await NodeContext.Set<Cash_tbCategoryExpSyntax>()
                .OrderBy(s => s.SyntaxTypeCode)
                .Select(s => new { s.SyntaxTypeCode, s.SyntaxType })
                .ToListAsync();

            SyntaxTypeItems = syntaxes.Select(s => new SelectListItem {
                Value = s.SyntaxTypeCode.ToString(),
                Text = s.SyntaxType,
                Selected = SyntaxTypeCode.HasValue && SyntaxTypeCode.Value == s.SyntaxTypeCode
            }).ToList();

            ExistingFormats = await NodeContext.Cash_tbCategoryExps
                .Select(e => e.Format)
                .Where(f => f != null && f != "")
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();
        }
    }
}
