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
    public class CreateExpressionModel : DI_BasePageModel
    {
        public CreateExpressionModel(NodeContext context) : base(context) { }

        [BindProperty]
        [Required]
        [StringLength(10)]
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [Display(Name = "Cash Type")]
        public short? CashTypeCode { get; set; }

        [BindProperty]
        [Required]
        [StringLength(256)]
        [Display(Name = "Expression")]
        public string Expression { get; set; } = string.Empty;

        [BindProperty]
        [StringLength(100)]
        [Display(Name = "Format")]
        public string Format { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [Display(Name = "Syntax Type")]
        public short? SyntaxTypeCode { get; set; }

        public IEnumerable<SelectListItem> CashTypeItems { get; private set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> SyntaxTypeItems { get; private set; } = Enumerable.Empty<SelectListItem>();
        public IList<string> ExistingFormats { get; private set; } = new List<string>();
        public IEnumerable<SelectListItem> FormatTemplateItems { get; private set; } = Enumerable.Empty<SelectListItem>();

        public bool OperationSucceeded { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadLookupsAsync();

            if (!CashTypeCode.HasValue)
            {
                short tradeCode = (short)NodeEnum.CashType.Trade;
                if (CashTypeItems.Any(i => i.Value == tradeCode.ToString()))
                    CashTypeCode = tradeCode;
            }

            if (!SyntaxTypeCode.HasValue && SyntaxTypeItems.Any(i => i.Value == "0"))
                SyntaxTypeCode = (short)NodeEnum.SyntaxType.Both;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await LoadLookupsAsync();

                if (!ModelState.IsValid)
                    return Page();

                if (!CashTypeCode.HasValue)
                {
                    ModelState.AddModelError(nameof(CashTypeCode), "Select a Cash Type.");
                    return Page();
                }

                if (!SyntaxTypeCode.HasValue)
                    SyntaxTypeCode = (short)NodeEnum.SyntaxType.Both;

                var syntax = (NodeEnum.SyntaxType)SyntaxTypeCode.Value;

                short expressionTypeCode = (short)NodeEnum.CategoryType.Expression;

                var categoryExists = await NodeContext.Cash_tbCategories
                    .AnyAsync(c => c.CategoryCode == CategoryCode);

                if (categoryExists)
                {
                    ModelState.AddModelError(nameof(CategoryCode),
                        "Category Code already exists. Expression categories cannot reuse an existing category.");
                    return Page();
                }

                var expressionExists = await NodeContext.Cash_tbCategoryExps
                    .AnyAsync(e => e.CategoryCode == CategoryCode);

                if (expressionExists)
                {
                    ModelState.AddModelError(nameof(CategoryCode),
                        "Expression already exists for this Category Code.");
                    return Page();
                }

                if (syntax == NodeEnum.SyntaxType.Both || syntax == NodeEnum.SyntaxType.LibreOffice)
                {
                    if (string.IsNullOrWhiteSpace(Format))
                    {
                        ModelState.AddModelError(nameof(Format),
                            "A format template is required for LibreOffice or Both syntax types.");
                        return Page();
                    }

                    var existsTemplate = await NodeContext.Set<Cash_tbCategoryExprFormat>()
                        .AsNoTracking()
                        .AnyAsync(t => t.TemplateCode == Format);

                    if (!existsTemplate)
                    {
                        ModelState.AddModelError(nameof(Format),
                            "Format must be a valid format template code.");
                        return Page();
                    }
                }
                // Excel: Format is free text and optional.

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                var newCategory = new Cash_tbCategory {
                    CategoryCode = CategoryCode,
                    Category = Category,
                    CategoryTypeCode = expressionTypeCode,
                    CashTypeCode = CashTypeCode.Value,
                    CashPolarityCode = (short)NodeEnum.CashPolarity.Neutral,
                    DisplayOrder = await NextExpressionDisplayOrderAsync(),
                    IsEnabled = 1
                };

                NodeContext.Cash_tbCategories.Add(newCategory);
                await NodeContext.SaveChangesAsync();

                var expression = new Cash_tbCategoryExp {
                    CategoryCode = CategoryCode,
                    Expression = Expression,
                    Format = Format ?? string.Empty,
                    IsError = false,
                    ErrorMessage = null,
                    SyntaxTypeCode = SyntaxTypeCode ?? (short)NodeEnum.SyntaxType.Both
                };

                NodeContext.Cash_tbCategoryExps.Add(expression);
                await NodeContext.SaveChangesAsync();

                await tx.CommitAsync();

                OperationSucceeded = true;

                var exprKey = CategoryTreeModel.MakeExpressionKey(CategoryCode);

                if (Request.Query["embed"] != "1")
                {
                    return RedirectToPage("/Cash/CategoryTree/Index", new {
                        select = exprKey,
                        key = exprKey,
                        expand = CategoryTreeModel.ExpressionsNodeKey
                    });
                }

                string exprPreview = Expression ?? string.Empty;
                if (exprPreview.Length > 60)
                    exprPreview = exprPreview[..57] + "...";

                var title =
                    "<span class='bi bi-calculator me-1'></span>" +
                    WebUtility.HtmlEncode(Category) +
                    " (" + WebUtility.HtmlEncode(CategoryCode) + ") " +
                    "<span class='tc-exp-formula'>= " + WebUtility.HtmlEncode(exprPreview) + "</span>";

                var nodeSpec = new {
                    key = exprKey,
                    title,
                    icon = false,
                    data = new {
                        nodeType = "expression",
                        categoryCode = CategoryCode,
                        category = Category,
                        cashTypeCode = CashTypeCode,
                        expression = Expression,
                        format = Format ?? string.Empty,
                        syntaxTypeCode = SyntaxTypeCode ?? (short)NodeEnum.SyntaxType.Both,
                        isEnabled = 1,
                        displayOrder = newCategory.DisplayOrder
                    }
                };

                ViewData["EmbedSelectKey"] = exprKey;
                ViewData["EmbedExpandKey"] = CategoryTreeModel.ExpressionsNodeKey;
                ViewData["EmbedNodeJson"] = JsonSerializer.Serialize(nodeSpec);

                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                throw;
            }
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

            var syntaxes = await NodeContext.Cash_tbCategoryExpSyntax
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

            var templates = await NodeContext.Set<Cash_tbCategoryExprFormat>()
                .OrderBy(t => t.TemplateCode)
                .Select(t => new { t.TemplateCode, t.Template, t.TemplateDescription })
                .ToListAsync();

            FormatTemplateItems = templates.Select(t => new SelectListItem
            {
                Value = t.TemplateCode,
                Text = string.IsNullOrWhiteSpace(t.TemplateDescription)
                    ? $"{t.TemplateCode} - {t.Template}"
                    : t.TemplateDescription
            }).ToList();

            var templateMap = templates.ToDictionary(
                t => t.TemplateCode,
                t => t.Template
            );

            ViewData["FormatTemplateMap"] = JsonSerializer.Serialize(templateMap);
        }

        private async Task<short> NextExpressionDisplayOrderAsync()
        {
            var max = await NodeContext.Cash_tbCategories
                .Where(c => c.CategoryTypeCode == (short)NodeEnum.CategoryType.Expression)
                .Select(c => (int?)c.DisplayOrder)
                .MaxAsync() ?? 0;

            return (short)(max + 1);
        }
    }
}
