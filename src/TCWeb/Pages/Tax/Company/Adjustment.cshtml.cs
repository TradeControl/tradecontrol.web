using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [Authorize(Roles = "Administrators, Managers")]
    public class AdjustmentModel : DI_BasePageModel
    {
        public AdjustmentModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public Cash_vwTaxCorpTotal Cash_CorpTaxTotal { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        public double TaxAdjustment { get; set; }

        [BindProperty]
        public DateTime StartOn { get; set; }

        public async Task<IActionResult> OnGetAsync(DateTime? startOn)
        {
            if (startOn == null)
                return NotFound();

            Cash_CorpTaxTotal = await NodeContext.Cash_TaxCorpTotals.FirstOrDefaultAsync(m => m.StartOn == startOn);

            if (Cash_CorpTaxTotal == null)
                return NotFound();
            else
            {
                TaxAdjustment = (double)NodeContext.App_tbYearPeriods.Where(p => p.YearNumber == Cash_CorpTaxTotal.YearNumber).Sum(p => p.TaxAdjustment);
                StartOn = (DateTime)startOn;

                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Data.FinancialPeriods periods = new(NodeContext);
                await periods.AdjustTax(StartOn, NodeEnum.TaxType.CorporationTax, TaxAdjustment);

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
