using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Register
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Invoice_vwRegisterDetail Invoice_Detail { get; set; }

        [BindProperty]
        public string PeriodName { get; set; }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber, string projectCode)
        {
            if (invoiceNumber == null || projectCode == null)
                return NotFound();

            Invoice_Detail = await NodeContext.Invoice_RegisterDetails.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && i.ProjectCode == projectCode);

            if (Invoice_Detail == null)
                return NotFound();
            else
            {
                FinancialPeriods periods = new(NodeContext);
                PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == Invoice_Detail.StartOn).Select(t => t.Description).FirstOrDefaultAsync();

                await SetViewData();
                return Page();
            }
        }
    }
}
