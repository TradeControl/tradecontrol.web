using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string taxCode)
        {
            try
            {
                if (string.IsNullOrEmpty(taxCode))
                    return NotFound();

                var tbTaxCode = await NodeContext.App_tbTaxCodes.FindAsync(taxCode);
                NodeContext.App_tbTaxCodes.Remove(tbTaxCode);
                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("taxTypeCode", tbTaxCode.TaxTypeCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
