using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditCashCodeModel : DI_BasePageModel
    {
        private ICategoryTreeChangeControlService CategoryTreeService { get; }

        public EditCashCodeModel(NodeContext context,
            ICategoryTreeChangeControlService categoryTreeService) : base(context)
        {
            CategoryTreeService = categoryTreeService;
        }

        [BindProperty(SupportsGet = true)]
        public string VersionId { get; set; } = "";

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

            var code = await (from c in NodeContext.Cash_tbCodes
                join map in NodeContext.Cash_tbCategoryCodeMaps on c.CashCode equals map.CashCode
                where map.VersionId == VersionId && c.CashCode == CashCode
                select new { c.CashCode, c.CashDescription, c.TaxCode, c.IsEnabled, map.CategoryCode })
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

            try
            {
                await CategoryTreeService.UpdateCashCodeAsync(new CategoryTreeCashCodeUpdateCommand(
                    VersionId, CashCode, CashDescription, TaxCode,
                    IsEnabled ? (short)1 : (short)0));
            }
            catch (System.Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ModelState.AddModelError(string.Empty, CategoryTreeCommandFeedback.From(ex));
                CategoryCode = await NodeContext.Cash_tbCategoryCodeMaps
                    .Where(map => map.VersionId == VersionId && map.CashCode == CashCode)
                    .Select(map => map.CategoryCode)
                    .SingleOrDefaultAsync() ?? "";
                await LoadTaxList(TaxCode);
                return Page();
            }
            OperationSucceeded = true;

            var nodeKey = "code:" + CashCode;
            var parentKey = await NodeContext.Cash_tbCategoryCodeMaps
                .Where(map => map.VersionId == VersionId && map.CashCode == CashCode)
                .Select(map => map.CategoryCode)
                .SingleAsync();

            var isEmbedded =
                string.Equals(Request.Query["embed"], "1", System.StringComparison.OrdinalIgnoreCase) ||
                (Request.HasFormContentType && string.Equals(Request.Form["embed"], "1", System.StringComparison.OrdinalIgnoreCase));

            if (isEmbedded)
            {
                ViewData["EditMarkerKey"] = nodeKey;
                ViewData["EditMarkerParent"] = parentKey;
                ViewData["EditMarkerDesc"] = CashDescription;
                ViewData["EditMarkerEnabled"] = IsEnabled ? (short)1 : (short)0;
                await LoadTaxList(TaxCode); // Keep list for potential re-render
                return Page();
            }

            // Full page redirect (mobile): include selection hints
            return RedirectToPage("/Cash/CategoryTree/Index",
                new { versionId = VersionId, select = nodeKey, expand = parentKey,
                    parentKey = parentKey, key = nodeKey });
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
