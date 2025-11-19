using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize(Roles = "Administrators")]
    public class EditCashCodeModel : DI_BasePageModel
    {
        public EditCashCodeModel(NodeContext context) : base(context) { }
        [BindProperty] public string CashCode { get; set; } = "";
        [BindProperty] public string CashDescription { get; set; } = "";
        [BindProperty] public bool IsEnabled { get; set; }
        [BindProperty] public string ParentKey { get; set; } = "";

        // optional: if editing influences title icon, we may track cash type
        public short CashTypeCode { get; private set; }

        public bool OperationSucceeded { get; private set; }
        public string ErrorMessage { get; private set; } = "";

        public async Task<IActionResult> OnGetAsync(string key, bool embed = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    ErrorMessage = "Missing key.";
                    return embed ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var codeRow = await NodeContext.Cash_tbCodes
                    .Join(NodeContext.Cash_tbCategories,
                          cd => cd.CategoryCode,
                          ct => ct.CategoryCode,
                          (cd, ct) => new { cd, ct })
                    .Where(x => x.cd.CashCode == key)
                    .Select(x => new {
                        x.cd.CashCode,
                        x.cd.CashDescription,
                        x.cd.IsEnabled,
                        Parent = x.cd.CategoryCode,
                        CashType = x.ct.CashTypeCode
                    })
                    .FirstOrDefaultAsync();

                if (codeRow == null)
                {
                    ErrorMessage = "Cash code not found.";
                    return embed ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                CashCode = codeRow.CashCode;
                CashDescription = codeRow.CashDescription;
                IsEnabled = codeRow.IsEnabled != 0;
                ParentKey = codeRow.Parent ?? "";
                CashTypeCode = codeRow.CashType;

                return Page();
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
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
                if (string.IsNullOrWhiteSpace(CashCode))
                {
                    ErrorMessage = "Missing key.";
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Missing key</div>") : Page();
                }

                var codeRow = await NodeContext.Cash_tbCodes
                    .FirstOrDefaultAsync(c => c.CashCode == CashCode);

                if (codeRow == null)
                {
                    ErrorMessage = "Cash code not found.";
                    return isEmbedded ? Content("<div class='text-danger small p-2'>Not found</div>") : Page();
                }

                if (!string.IsNullOrWhiteSpace(CashDescription))
                {
                    codeRow.CashDescription = CashDescription.Trim();
                }
                codeRow.IsEnabled = IsEnabled ? (short)1 : (short)0;

                NodeContext.Attach(codeRow).State = EntityState.Modified;
                await NodeContext.SaveChangesAsync();

                // resolve current parent + cash type for marker/title
                var parentType = await NodeContext.Cash_tbCategories
                    .Where(c => c.CategoryCode == codeRow.CategoryCode)
                    .Select(c => new { c.CashTypeCode })
                    .FirstOrDefaultAsync();

                ParentKey = codeRow.CategoryCode;
                CashTypeCode = parentType?.CashTypeCode ?? 0;
                OperationSucceeded = true;

                if (isEmbedded)
                {
                    return Page();
                }

                return RedirectToPage("./Index", new { key = "code:" + CashCode, select = "code:" + CashCode, expand = ParentKey });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                ErrorMessage = "Server error.";
                return isEmbedded ? Content("<div class='text-danger small p-2'>Server error</div>") : Page();
            }
        }
    }
}
