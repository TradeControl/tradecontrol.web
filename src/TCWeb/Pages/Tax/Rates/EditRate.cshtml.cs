using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Tax.Rates
{
    [Authorize(Roles = "Administrators")]
    public class EditRateModel : DI_BasePageModel
    {
        public EditRateModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public short? YearNumber { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime StartOn { get; set; } = DateTime.Today;

        [BindProperty]
        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime EndOn { get; set; } = DateTime.Today;

        [BindProperty]
        [Display(Name = "Corporation Tax Rate (%)")]
        public float CorporationTaxRatePercent { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (YearNumber.HasValue)
            {
                var firstPeriodStart = await NodeContext.App_tbYearPeriods
                    .Where(p => p.YearNumber == YearNumber.Value)
                    .OrderBy(p => p.StartOn)
                    .Select(p => (DateTime?)p.StartOn)
                    .FirstOrDefaultAsync();

                var lastPeriodStart = await NodeContext.App_tbYearPeriods
                    .Where(p => p.YearNumber == YearNumber.Value)
                    .OrderByDescending(p => p.StartOn)
                    .Select(p => (DateTime?)p.StartOn)
                    .FirstOrDefaultAsync();

                if (firstPeriodStart.HasValue && lastPeriodStart.HasValue)
                {
                    StartOn = firstPeriodStart.Value;
                    EndOn = lastPeriodStart.Value;
                }
            }

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await SetViewData();

                if (!ModelState.IsValid)
                    return Page();

                var embedded = Request.Form.TryGetValue("embedded", out var emb) && emb == "1" ? "1" : null;
                var returnNode = Request.Form.TryGetValue("returnNode", out var rn) ? rn.ToString() : "TaxCompany";

                var periods = new FinancialPeriods(NodeContext);

                await periods.TaxRate(StartOn, EndOn, CorporationTaxRatePercent / 100f);

                return RedirectToPage("./Index", new {
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
