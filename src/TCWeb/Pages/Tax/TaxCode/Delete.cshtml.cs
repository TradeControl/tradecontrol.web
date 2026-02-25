using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.TaxCode
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

        public App_vwTaxCode App_TaxCode { get; set; }

        public async Task<IActionResult> OnGetAsync(string taxCode)
        {
            if (string.IsNullOrEmpty(taxCode))
                return NotFound();

            App_TaxCode = await NodeContext.App_TaxCodes.FirstOrDefaultAsync(m => m.TaxCode == taxCode);
            if (App_TaxCode == null)
                return NotFound();

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string taxCode, string embedded, string returnNode, string taxType, string searchString)
        {
            try
            {
                if (string.IsNullOrEmpty(taxCode))
                    return NotFound();

                var embeddedMode = string.Equals(embedded, "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(embedded, "true", StringComparison.OrdinalIgnoreCase);

                returnNode = string.IsNullOrWhiteSpace(returnNode) ? "TaxCode" : returnNode;

                var tbTaxCode = await NodeContext.App_tbTaxCodes.FindAsync(taxCode);
                if (tbTaxCode != null)
                {
                    NodeContext.App_tbTaxCodes.Remove(tbTaxCode);
                    await NodeContext.SaveChangesAsync();
                }

                return RedirectToPage("./Index", new {
                    embedded = embeddedMode ? "1" : null,
                    returnNode,
                    taxType,
                    searchString
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
