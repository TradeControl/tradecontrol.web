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

namespace TradeControl.Web.Pages.Tax.Company
{
    public class LossesCarriedForwardModel : DI_BasePageModel
    {
        public LossesCarriedForwardModel(NodeContext context) : base(context) { }

        public IList<Cash_vwTaxLossesCarriedForward> Cash_LossesCarriedForward { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                Cash_LossesCarriedForward = await NodeContext.Cash_TaxLossesCarriedForward.OrderBy(t => t.StartOn).ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}