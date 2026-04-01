using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Company
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

        public Cash_vwTaxBizTotal Cash_TaxBizTotal { get; set; }

        public async Task<IActionResult> OnGetAsync(DateTime? startOn)
        {
            if (startOn == null)
                return NotFound();

            Cash_TaxBizTotal = await NodeContext.Cash_TaxBizTotals.FirstOrDefaultAsync(m => m.StartOn == startOn);

            if (Cash_TaxBizTotal == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}