using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditCashCodeModel : DI_BasePageModel
    {
        public EditCashCodeModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string CashCode { get; set; } = "";

        [BindProperty]
        public string CashDescription { get; set; } = "";

        [BindProperty]
        public string TaxCode { get; set; } = "";

        [BindProperty]
        public bool IsEnabled { get; set; }

        public string CategoryCode { get; private set; } = "";
        public SelectList TaxCodes { get; private set; }
        public bool OperationSucceeded { get; private set; }

        // GET
        public async Task<IActionResult> OnGetAsync()
        {
            // Accept either key=code:XYZ or key=XYZ
            if (string.IsNullOrWhiteSpace(CashCode))
            {
                var keyRaw = Request.Query["key"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(keyRaw))
                {
                    CashCode = keyRaw.StartsWith("code:") ? keyRaw.Substring(5) : keyRaw;
                }
            }

            if (string.IsNullOrWhiteSpace(CashCode))
                return NotFound();

            var code = await NodeContext.Cash_tbCodes
                .Where(c => c.CashCode == CashCode)
                .Select(c => new { c.CashCode, c.CashDescription, c.TaxCode, c.IsEnabled, c.CategoryCode })
                .FirstOrDefaultAsync();

            if (code == null)
                return NotFound();

            CashDescription = code.CashDescription;
            TaxCode = code.TaxCode;
            IsEnabled = code.IsEnabled != 0;
            CategoryCode = code.CategoryCode;

            await LoadTaxList(code.TaxCode);
            return Page();
        }

        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CashCode))
            {
                // Fallback to query/form key again
                var keyRaw = Request.Query["key"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(keyRaw))
                    CashCode = keyRaw.StartsWith("code:") ? keyRaw.Substring(5) : keyRaw;
            }

            if (string.IsNullOrWhiteSpace(CashCode))
                return NotFound();

            var code = await NodeContext.Cash_tbCodes
                .Where(c => c.CashCode == CashCode)
                .FirstOrDefaultAsync();

            if (code == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadTaxList(TaxCode);
                return Page();
            }

            // Validate selected tax
            if (!await NodeContext.App_tbTaxCodes.AnyAsync(t => t.TaxCode == TaxCode))
            {
                ModelState.AddModelError(nameof(TaxCode), "Invalid Tax Code.");
                await LoadTaxList(TaxCode);
                return Page();
            }

            code.CashDescription = CashDescription;
            code.TaxCode = TaxCode;
            code.IsEnabled = IsEnabled ? (short)1 : (short)0;

            await NodeContext.SaveChangesAsync();
            OperationSucceeded = true;

            var nodeKey = "code:" + CashCode;
            var parentKey = code.CategoryCode;

            var isEmbedded =
                string.Equals(Request.Query["embed"], "1", System.StringComparison.OrdinalIgnoreCase) ||
                (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", System.StringComparison.OrdinalIgnoreCase));

            if (isEmbedded)
            {
                ViewData["EditMarkerKey"] = nodeKey;
                ViewData["EditMarkerParent"] = parentKey;
                ViewData["EditMarkerDesc"] = CashDescription;
                ViewData["EditMarkerEnabled"] = code.IsEnabled;
                await LoadTaxList(TaxCode); // Keep list for potential re-render
                return Page();
            }

            // Full page redirect (mobile): include selection hints
            return RedirectToPage("/Cash/CategoryTree/Index",
                new { select = nodeKey, expand = parentKey, parentKey = parentKey, key = nodeKey });
        }

        private async Task LoadTaxList(string selected)
        {
            var taxes = await NodeContext.App_tbTaxCodes
                .OrderBy(t => t.TaxCode)
                .Select(t => new { t.TaxCode })
                .ToListAsync();
            TaxCodes = new SelectList(taxes, "TaxCode", "TaxCode", selected);
        }
    }
}
