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

namespace TradeControl.Web.Pages.Tax.Vat
{
    [Authorize(Roles = "Administrators, Managers")]
    public class AdjustmentModel : DI_BasePageModel
    {
        public AdjustmentModel(NodeContext context) : base(context) { }

        public Cash_vwTaxVatTotal Cash_VatTotal { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        public double VatAdjustment { get; set; }

        [BindProperty]
        public DateTime StartOn { get; set; }

        public async Task<IActionResult> OnGetAsync(DateTime? startOn)
        {
            if (startOn == null)
                return NotFound();

            Cash_VatTotal = await NodeContext.Cash_TaxVatTotals.FirstOrDefaultAsync(m => m.StartOn == startOn);

            if (Cash_VatTotal == null)
                return NotFound();
            else
            {
                VatAdjustment = Cash_VatTotal.VatAdjustment;
                StartOn = Cash_VatTotal.StartOn;

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
                await periods.AdjustTax(StartOn, NodeEnum.TaxType.VAT, VatAdjustment);

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