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

        public Cash_vwTaxCorpTotal Cash_CorpTaxTotal { get; set; }

        public async Task<IActionResult> OnGetAsync(DateTime? startOn)
        {
            if (startOn == null)
                return NotFound();

            Cash_CorpTaxTotal = await NodeContext.Cash_TaxCorpTotals.FirstOrDefaultAsync(m => m.StartOn == startOn);

            if (Cash_CorpTaxTotal == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}