using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;


namespace TradeControl.Web.Pages.Tax.Vat
{
    public class StatementModel : DI_BasePageModel
    {
        public StatementModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<Cash_vwTaxVatStatement> Cash_VatStatement { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                Cash_VatStatement = await NodeContext.Cash_TaxVatStatement.OrderByDescending(t => t.RowNumber).ToListAsync();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
