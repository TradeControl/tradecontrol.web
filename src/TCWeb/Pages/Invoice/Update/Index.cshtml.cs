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

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Invoice_vwRegister> Invoice_Headers { get; set; }

        // Filters (bind to GET so cshtml selects work)
        [BindProperty(SupportsGet = true)]
        public string AccountCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? Printed { get; set; }

        [BindProperty(SupportsGet = true)]
        public string InvoiceNumber { get; set; }

        [BindProperty(SupportsGet = true)]
        public string InvoiceType { get; set; }

        public SelectList InvoiceTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PeriodName { get; set; }

        public SelectList PeriodNames { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 50;     // default 50

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Populate selects first so we can default PeriodName if needed
                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());

                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes
                    .OrderBy(t => t.InvoiceTypeCode)
                    .Select(t => t.InvoiceType)
                    .ToListAsync());

                // Base query
                IQueryable<Invoice_vwRegister> invoices = from tb in NodeContext.Invoice_Register select tb;

                // Determine period selection and apply period filter unless AccountCode or InvoiceNumber specified
                DateTime startOn = DateTime.Today;

                if (!string.IsNullOrEmpty(AccountCode))
                {
                    invoices = invoices.Where(i => i.SubjectCode == AccountCode);
                    PeriodName = null;
                }
                else if (!string.IsNullOrEmpty(InvoiceNumber))
                {
                    invoices = invoices.Where(i => i.InvoiceNumber == InvoiceNumber);

                    // determine period for this invoice
                    DateTime start = await invoices.Select(i => i.StartOn).FirstOrDefaultAsync();
                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == start).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                {
                    if (string.IsNullOrEmpty(PeriodName))
                    {
                        var periods = new Data.FinancialPeriods(NodeContext);
                        startOn = periods.ActiveStartOn;
                        PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                    }
                    else
                    {
                        startOn = await NodeContext.App_Periods.Where(t => t.Description == PeriodName).Select(t => t.StartOn).FirstOrDefaultAsync();
                    }

                    invoices = invoices.Where(i => i.StartOn == startOn);
                }

                // Apply additional filters
                if (!string.IsNullOrEmpty(InvoiceType))
                    invoices = invoices.Where(i => i.InvoiceType == InvoiceType);

                if (Printed.HasValue && !Printed.Value)
                    invoices = invoices.Where(i => !i.Printed);

                // Page size options (10, 50, 100)
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());

                // protect PageSize
                if (PageSize <= 0) PageSize = 50;

                // compute totals BEFORE paging
                TotalItems = await invoices.CountAsync();

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                Invoice_Headers = await invoices
                    .OrderByDescending(i => i.InvoicedOn)
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

        public async Task<IActionResult> OnPostMarkAllAsSent()
        {
            try
            {
                Docs docs = new(NodeContext);
                await docs.DespoolAll();
                RouteValueDictionary route = new();
                route.Add("Printed", false);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
