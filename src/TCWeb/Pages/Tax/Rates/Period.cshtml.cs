using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Rates
{
    [Authorize(Roles = "Administrators, Managers")]
    public class PeriodModel : DI_BasePageModel
    {
        public PeriodModel(NodeContext context) : base(context) { }

        public App_tbYearPeriod YearPeriod { get; private set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime StartOn { get; set; }

        [BindProperty(SupportsGet = true)]
        public short? YearNumber { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "VAT Adjustment")]
        public decimal VatAdjustment { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Corporation Tax Adjustment")]
        public decimal CorporationTaxAdjustment { get; set; }

        [BindProperty]
        [Display(Name = "Corporation Tax Rate (%)")]
        public float CorporationTaxRatePercent { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            YearPeriod = await NodeContext.App_tbYearPeriods.SingleOrDefaultAsync(p => p.StartOn == StartOn);
            if (YearPeriod == null)
                return NotFound();

            if (!YearNumber.HasValue)
                YearNumber = YearPeriod.YearNumber;

            // The adjustment is stored on the *window closing period*, not necessarily this month.
            VatAdjustment = await NodeContext.TaxAdjustmentGet(StartOn, NodeEnum.TaxType.VAT);
            CorporationTaxAdjustment = await NodeContext.TaxAdjustmentGet(StartOn, NodeEnum.TaxType.CorporationTax);

            CorporationTaxRatePercent = YearPeriod.CorporationTaxRate * 100f;

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAdjustmentsAsync(string embedded, string returnNode)
        {
            try
            {
                if (!ModelState.IsValid)
                    return await OnGetAsync();

                var periods = new FinancialPeriods(NodeContext);

                await periods.AdjustTax(StartOn, NodeEnum.TaxType.VAT, (double)VatAdjustment);
                await periods.AdjustTax(StartOn, NodeEnum.TaxType.CorporationTax, (double)CorporationTaxAdjustment);

                return RedirectToPage("./Index", new
                {
                    embedded,
                    returnNode,
                    yearNumber = YearNumber
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostSaveRateAsync(string embedded, string returnNode)
        {
            try
            {
                if (!ModelState.IsValid)
                    return await OnGetAsync();

                var period = await NodeContext.App_tbYearPeriods.SingleOrDefaultAsync(p => p.StartOn == StartOn);
                if (period == null)
                    return NotFound();

                period.CorporationTaxRate = CorporationTaxRatePercent / 100f;
                await NodeContext.SaveChangesAsync();

                return RedirectToPage("./Index", new
                {
                    embedded,
                    returnNode,
                    yearNumber = YearNumber
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
