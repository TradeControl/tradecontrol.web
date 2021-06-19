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

namespace TradeControl.Web.Pages.Tax.Vat.Periods
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

        public Cash_vwTaxVatSummary Cash_VatSummary { get; set; }

        [BindProperty]
        public string PeriodName { get; set; }

        public async Task<IActionResult> OnGetAsync(DateTime? startOn, string taxCode)
        {
            if (startOn == null || taxCode == null)
                return NotFound();

            PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
            Cash_VatSummary = await NodeContext.Cash_TaxVatSummary.FirstOrDefaultAsync(m => m.StartOn == startOn & m.TaxCode == taxCode);

            if (Cash_VatSummary == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}