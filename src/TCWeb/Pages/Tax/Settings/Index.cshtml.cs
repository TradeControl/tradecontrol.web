using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Settings
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Cash_vwTaxType> App_TaxTypes { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();
                App_TaxTypes = await NodeContext.App_TaxTypes.OrderBy(t => t.TaxTypeCode).ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
