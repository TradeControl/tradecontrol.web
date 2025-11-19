using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class DeleteCashCodeModel : DI_BasePageModel
    {
        public DeleteCashCodeModel(NodeContext nodeContext) : base(nodeContext)
        {
        }

        // ?key=code:CC123 or ?key=CC123 (tree node key)
        [BindProperty(SupportsGet = true)]
        public string Key { get; set; } = "";

        // ?cashCode=CC123 (optional) and POST hidden field binding
        [BindProperty]
        public string CashCode { get; set; } = "";

        public string CodeDisplay { get; private set; } = "";
        public string ParentCategoryCode { get; private set; } = "";
        public bool OperationSucceeded { get; private set; } = false;

        private string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) { return ""; }
            return value.StartsWith("code:", StringComparison.OrdinalIgnoreCase)
                ? value.Substring(5)
                : value;
        }

        private void ResolveCashCode()
        {
            // Prefer posted CashCode (on POST); otherwise derive from Key
            var candidate = !string.IsNullOrWhiteSpace(CashCode) ? CashCode : Key;
            CashCode = Normalize(candidate);
        }

        public async Task OnGetAsync()
        {
            ResolveCashCode();

            if (!string.IsNullOrWhiteSpace(CashCode))
            {
                var code = await NodeContext.Cash_tbCodes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CashCode == CashCode);

                if (code != null)
                {
                    CodeDisplay = $"{code.CashCode} - {code.CashDescription}";
                    ParentCategoryCode = code.CategoryCode;
                }
                else
                {
                    CodeDisplay = CashCode;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ResolveCashCode();

            if (string.IsNullOrWhiteSpace(CashCode))
            {
                ModelState.AddModelError(string.Empty, "Missing key.");
                return Page();
            }

            try
            {
                var code = await NodeContext.Cash_tbCodes
                    .FirstOrDefaultAsync(c => c.CashCode == CashCode);

                if (code == null)
                {
                    OperationSucceeded = true;
                    return Page(); // treat as success (phantom node)
                }

                ParentCategoryCode = code.CategoryCode;

                await using var tx = await NodeContext.Database.BeginTransactionAsync();
                NodeContext.Cash_tbCodes.Remove(code);
                await NodeContext.SaveChangesAsync();
                await tx.CommitAsync();

                OperationSucceeded = true;

                var isEmbedded =
                    string.Equals(Request.Query["embed"], "1", StringComparison.Ordinal) ||
                    string.Equals(Request.Form["embed"], "1", StringComparison.Ordinal) ||
                    string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

                if (isEmbedded)
                {
                    return Page(); // JS will remove node & select parent
                }

                // Full-page (mobile / non-embedded) redirect to parent selection
                if (!string.IsNullOrWhiteSpace(ParentCategoryCode))
                {
                    return RedirectToPage("/Cash/CategoryTree/Index", new {
                        select = ParentCategoryCode,
                        parentKey = ParentCategoryCode,
                        expand = ParentCategoryCode
                    });
                }

                return RedirectToPage("/Cash/CategoryTree/Index");
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ModelState.AddModelError(string.Empty, "Delete failed (server error).");
                return Page();
            }
        }
    }
}
