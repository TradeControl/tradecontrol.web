using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Settings
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

        public Cash_vwTaxType App_TaxType { get; set; }

        public async Task<IActionResult> OnGetAsync(short? taxTypeCode)
        {
            if (taxTypeCode == null)
                return NotFound();

            App_TaxType = await NodeContext.App_TaxTypes.FirstOrDefaultAsync(m => m.TaxTypeCode == taxTypeCode);

            if (App_TaxType == null)
                return NotFound();

            await SetViewData();
            return Page();
        }
    }
}
