using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.TaxCode
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

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
    }
}
