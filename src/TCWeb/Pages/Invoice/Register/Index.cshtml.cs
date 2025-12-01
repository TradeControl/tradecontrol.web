using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Register
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Invoice_vwRegisterDetail> Invoice_Details { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty(SupportsGet = true)]
        public string InvoiceType { get; set; }
        public SelectList InvoiceTypes { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Invoice Total")]
        public double TotalInvoiceValue { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Tax Total")]
        public double TotalTaxValue { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;     // default 10

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync(string invoiceType, string accountCode, string invoiceNumber)
        {
            try
            {
                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());

                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());

                // Base query
                IQueryable<Invoice_vwRegisterDetail> invoices = from tb in NodeContext.Invoice_RegisterDetails select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    invoices = invoices.Where(i => i.SubjectCode == accountCode);
                    PeriodName = null;
                }
                else if (!string.IsNullOrEmpty(invoiceNumber))
                {
                    invoices = invoices.Where(i => i.InvoiceNumber == invoiceNumber);

                    FinancialPeriods periods = new(NodeContext);
                    DateTime startOn = await invoices.Select(i => i.StartOn).FirstOrDefaultAsync();
                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                {
                    DateTime startOn = DateTime.Today;

                    if (string.IsNullOrEmpty(PeriodName))
                    {
                        FinancialPeriods periods = new(NodeContext);
                        startOn = periods.ActiveStartOn;
                        PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                    }
                    else
                        startOn = await NodeContext.App_Periods.Where(t => t.Description == PeriodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                    invoices = invoices.Where(i => i.StartOn == startOn);
                }

                if (!string.IsNullOrEmpty(invoiceType))
                    invoices = invoices.Where(i => i.InvoiceType == invoiceType);

                // Page size options (10, 50, 100)
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());

                // protect PageSize
                if (PageSize <= 0) PageSize = 10;

                // compute totals BEFORE paging (use filtered query)
                TotalItems = await invoices.CountAsync();

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                // totals (from filtered query)
                TotalInvoiceValue = await invoices.SumAsync(i => i.InvoiceValue);
                TotalTaxValue = await invoices.SumAsync(i => i.TaxValue);

                // fetch paged data
                Invoice_Details = await invoices
                    .OrderBy(i => i.InvoicedOn)
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
