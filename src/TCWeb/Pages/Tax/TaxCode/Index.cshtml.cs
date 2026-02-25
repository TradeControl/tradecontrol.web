using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.TaxCode
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<App_vwTaxCode> App_TaxCodes { get; set; }
        public SelectList TaxTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TaxType { get; set; }

        public async Task OnGetAsync(short? taxTypeCode, string taxCode)
        {
            try
            {
                await SetViewData();

                var taxtypes = from tb in NodeContext.App_TaxCodeTypes
                               orderby tb.TaxType
                               select tb.TaxType;

                TaxTypes = new SelectList(await taxtypes.ToListAsync());

                var taxCodes = from tb in NodeContext.App_TaxCodes
                               select tb;

                if (!string.IsNullOrEmpty(taxCode))
                    taxTypeCode = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == taxCode).Select(t => t.TaxTypeCode).FirstOrDefaultAsync();

                if (taxTypeCode != null)
                    TaxType = await NodeContext.Cash_tbTaxTypes.Where(t => t.TaxTypeCode == taxTypeCode).Select(t => t.TaxType).FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(TaxType))
                    taxCodes = taxCodes.Where(t => t.TaxType == TaxType);

                App_TaxCodes = await taxCodes.ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
