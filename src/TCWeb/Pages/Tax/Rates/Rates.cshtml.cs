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


namespace TradeControl.Web.Pages.Tax.Rates
{
    [Authorize(Roles = "Administrators")]
    public class RatesModel : DI_BasePageModel
    {
        public RatesModel(NodeContext context) : base(context) {}

        [BindProperty]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime StartOn { get; set; } = DateTime.Today;

        [BindProperty]
        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime EndOn { get; set; } = DateTime.Today;

        [BindProperty]
        [Display(Name = "Tax Rate")]
        [DisplayFormat(DataFormatString = "{0:p}")]
        public float CorporationTaxRate { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync()
        {
            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Data.FinancialPeriods periods = new(NodeContext);
                await periods.TaxRate(StartOn, EndOn, CorporationTaxRate);

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
