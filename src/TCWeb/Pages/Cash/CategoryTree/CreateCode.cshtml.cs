using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class CreateCodeModel : DI_BasePageModel
    {
        public CreateCodeModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string CategoryCode { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string Key { get => CategoryCode; set => CategoryCode = value; }

        [BindProperty]
        public string CashCode { get; set; } = "";

        [BindProperty]
        public string CashDescription { get; set; } = "";

        [BindProperty]
        public string TaxCode { get; set; } = "";

        [BindProperty]
        public bool IsEnabled { get; set; } = true;

        public SelectList TaxCodes { get; private set; }

        public bool OperationSucceeded { get; set; } = false;

        public string NewCashCode { get; set; } = "";

        // New marker metadata for embedded response
        public string NewKey { get; private set; } = "";
        public string NewParentKey { get; private set; } = "";
        public string NewName { get; private set; } = "";
        public short NewPolarity { get; private set; }
        public short NewCategoryType { get; private set; }
        public short NewCashType { get; private set; }
        public short NewIsEnabled { get; private set; }

        public async Task OnGetAsync()
        {
            var taxes = await NodeContext.App_tbTaxCodes
                .OrderBy(t => t.TaxCode)
                .Select(t => new { t.TaxCode })
                .ToListAsync();

            TaxCodes = new SelectList(taxes, "TaxCode", "TaxCode");

            // Set default TaxCode to the business default when not provided (important for calculations).
            // Only set when TaxCode is empty so validation/posted values are not overwritten.
            if (string.IsNullOrWhiteSpace(TaxCode))
            {
                try
                {
                    var homeAccount = await NodeContext.App_HomeAccount
                        .Select(s => s.SubjectCode)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(homeAccount))
                    {
                        var defaultTax = await NodeContext.SubjectTaxCodeDefault(homeAccount);
                        if (!string.IsNullOrWhiteSpace(defaultTax))
                        {
                            TaxCode = defaultTax;
                        }
                    }
                }
                catch
                {
                    // swallow errors — we don't want GET to fail for this lookup.
                }
            }

            // Defensive: ensure CategoryCode is populated from any reasonable source so the hidden field is rendered.
            if (string.IsNullOrWhiteSpace(CategoryCode))
            {
                // Check common query keys and route values used by the UI (key, parentKey, category, categoryCode)
                string val = null;

                if (Request?.Query != null)
                {
                    val = Request.Query["key"].FirstOrDefault()
                          ?? Request.Query["parentKey"].FirstOrDefault()
                          ?? Request.Query["category"].FirstOrDefault()
                          ?? Request.Query["categoryCode"].FirstOrDefault()
                          ?? Request.Query["CategoryCode"].FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(val) && RouteData?.Values != null)
                {
                    if (RouteData.Values.TryGetValue("key", out var routeKey))
                    {
                        val = routeKey?.ToString();
                    }
                    else if (RouteData.Values.TryGetValue("category", out var routeCat))
                    {
                        val = routeCat?.ToString();
                    }
                }

                if (!string.IsNullOrWhiteSpace(val))
                {
                    CategoryCode = val;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // repopulate select list for validation failure but do not overwrite bound values
            await OnGetAsync();

            // Defensive: if model binding didn't populate CategoryCode (rare), pull it from the posted form values.
            if (string.IsNullOrWhiteSpace(CategoryCode) && Request?.HasFormContentType == true)
            {
                var f = Request.Form;
                CategoryCode = f["CategoryCode"].FirstOrDefault()
                               ?? f["key"].FirstOrDefault()
                               ?? f["Key"].FirstOrDefault()
                               ?? f["category"].FirstOrDefault()
                               ?? f["categoryCode"].FirstOrDefault();
            }

            // Provide field-level validation errors so they render in the embedded form.
            if (string.IsNullOrWhiteSpace(CategoryCode) || string.IsNullOrWhiteSpace(CashCode) || string.IsNullOrWhiteSpace(CashDescription))
            {
                // Add a model-level error so the validation summary (All) shows a clear message
                ModelState.AddModelError(string.Empty, "Category, code and description are required.");

                if (string.IsNullOrWhiteSpace(CategoryCode))
                {
                    ModelState.AddModelError(nameof(CategoryCode), "Category is required.");
                }

                if (string.IsNullOrWhiteSpace(CashCode))
                {
                    ModelState.AddModelError(nameof(CashCode), "Cash code is required.");
                }

                if (string.IsNullOrWhiteSpace(CashDescription))
                {
                    ModelState.AddModelError(nameof(CashDescription), "Description is required.");
                }

                return Page();
            }

            try
            {
                var catExists = await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == CategoryCode && c.IsEnabled != 0);
                if (!catExists)
                {
                    ModelState.AddModelError(nameof(CategoryCode), "Category not found or disabled.");
                    return Page();
                }

                if (await NodeContext.Cash_tbCodes.AnyAsync(c => c.CashCode == CashCode))
                {
                    ModelState.AddModelError(nameof(CashCode), "Cash code already exists.");
                    return Page();
                }

                string finalTax = TaxCode;
                if (string.IsNullOrWhiteSpace(finalTax))
                {
                    var anyTax = await NodeContext.App_tbTaxCodes.FirstOrDefaultAsync();
                    if (anyTax == null)
                    {
                        ModelState.AddModelError(nameof(TaxCode), "No tax codes available.");
                        return Page();
                    }
                    finalTax = anyTax.TaxCode;
                }

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                var code = new Cash_tbCode
                {
                    CashCode = CashCode,
                    CashDescription = CashDescription,
                    CategoryCode = CategoryCode,
                    TaxCode = finalTax,
                    IsEnabled = IsEnabled ? (short)1 : (short)0
                };

                NodeContext.Cash_tbCodes.Add(code);
                await NodeContext.SaveChangesAsync();

                // load parent category metadata for marker (polarity/type/cash type)
                var parent = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == CategoryCode)
                    .Select(c => new { c.CashPolarityCode, c.CategoryTypeCode, c.CashTypeCode })
                    .SingleOrDefaultAsync();

                await tx.CommitAsync();

                OperationSucceeded = true;
                NewCashCode = CashCode;

                // Populate marker fields (values the client expects)
                NewKey = $"code:{code.CashCode}";
                NewParentKey = CategoryCode;
                NewName = code.CashDescription;
                NewPolarity = parent?.CashPolarityCode ?? (short)NodeEnum.CashPolarity.Neutral;
                NewCategoryType = parent?.CategoryTypeCode ?? (short)NodeEnum.CategoryType.CashCode;
                NewCashType = parent?.CashTypeCode ?? 0;
                NewIsEnabled = code.IsEnabled;

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