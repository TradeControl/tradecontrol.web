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

namespace TradeControl.Web.Pages.Tax.Vat.Periods
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Cash_vwTaxVatSummary> Cash_VatSummary { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }


        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Vat Due")]
        public double TotalTaxValue { get; set; }

        // Pagination (monthly pages in 12s)
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 24;     // default 24

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync(string periodName)
        {
            try
            {
                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());

                DateTime startOn = DateTime.Today;

                if (string.IsNullOrEmpty(periodName))
                {
                    Data.FinancialPeriods periods = new(NodeContext);
                    startOn = periods.ActiveStartOn;
                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                    startOn = await NodeContext.App_Periods.Where(t => t.Description == periodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                var vat = from tb in NodeContext.Cash_TaxVatSummary
                          where tb.StartOn == startOn
                          select tb;

                // Page size options (12, 24, 48)
                PageSizeOptions = new SelectList(new[] { "12", "24", "48" }, PageSize.ToString());

                // ensure sensible PageSize
                if (PageSize <= 0) PageSize = 24;

                // compute totals BEFORE paging
                TotalItems = await vat.CountAsync();
                TotalTaxValue = await vat.SumAsync(i => i.VatDue);

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                Cash_VatSummary = await vat
                    .OrderBy(t => t.TaxCode)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}

