using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Totals
{
    public class IndexModel : DI_BasePageModel
    {
        public IList<Cash_Total> Cash_Totals { get; set; }

        public IndexModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync()
        {
            try
            {
                IQueryable<Cash_Total> cash_totals = from cat_total in NodeContext.Cash_CategoryTotals
                                                     join cash_type in NodeContext.Cash_tbTypes on cat_total.CashTypeCode equals cash_type.CashTypeCode
                                                     select new Cash_Total
                                                     {
                                                         CategoryCode = cat_total.CategoryCode,
                                                         Category = cat_total.Category,
                                                         CashType = cash_type.CashType,
                                                         DisplayOrder = cat_total.DisplayOrder,
                                                         InsertedBy = cat_total.InsertedBy,
                                                         InsertedOn = cat_total.InsertedOn,
                                                         UpdatedBy = cat_total.UpdatedBy,
                                                         UpdatedOn = cat_total.UpdatedOn
                                                     };

                Cash_Totals = await cash_totals.OrderBy(t => t.DisplayOrder).ToListAsync();

                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
