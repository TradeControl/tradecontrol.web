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
    public class UnpaidModel : DI_BasePageModel
    {
        public UnpaidModel(NodeContext context) : base(context) { }
        public IList<Invoice_vwRegisterOverdue> Invoice_RegisterOverdue { get; set; }

        [BindProperty(SupportsGet = true)]
        public string InvoiceType { get; set; }
        public SelectList InvoiceTypes { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Invoice Total")]
        public double TotalInvoiceValue { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Unpaid Total")]
        public double TotalPaidValue { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;    // default 10

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync(string invoiceType)
        {
            try
            {
                await SetViewData();

                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());

                // Base query
                IQueryable<Invoice_vwRegisterOverdue> invoices = from tb in NodeContext.Invoice_RegisterOverdue select tb;

                if (!string.IsNullOrEmpty(invoiceType))
                    invoices = invoices.Where(i => i.InvoiceType == invoiceType);

                // Page size options (10, 50, 100)
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());

                // protect PageSize
                if (PageSize <= 0) PageSize = 10;

                // compute totals BEFORE paging (from filtered query)
                TotalItems = await invoices.CountAsync();

                TotalInvoiceValue = (double)await invoices.SumAsync(i => i.InvoiceValue + i.TaxValue);
                TotalPaidValue = (double)await invoices.SumAsync(i => i.UnpaidValue);

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                Invoice_RegisterOverdue = await invoices
                    .OrderBy(i => i.ExpectedOn)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
