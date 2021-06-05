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
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

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

        public async Task OnGetAsync(string periodName, string invoiceType, string accountCode, string invoiceNumber)
        {
            try
            {
                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());

                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());

                var invoices = from tb in NodeContext.Invoice_RegisterDetails select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    invoices = invoices.Where(i => i.AccountCode == accountCode);
                    PeriodName = null;
                }
                else if (!string.IsNullOrEmpty(invoiceNumber))
                {
                    invoices = invoices.Where(i => i.InvoiceNumber == invoiceNumber);

                    Periods periods = new(NodeContext);
                    DateTime startOn = await invoices.Select(i => i.StartOn).FirstOrDefaultAsync();
                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                {
                    DateTime startOn = DateTime.Today;

                    if (string.IsNullOrEmpty(periodName))
                    {
                        Periods periods = new(NodeContext);
                        startOn = periods.ActiveStartOn;
                        PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                    }
                    else
                        startOn = await NodeContext.App_Periods.Where(t => t.Description == periodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                    invoices = invoices.Where(i => i.StartOn == startOn);
                }

                if (!string.IsNullOrEmpty(invoiceType))
                    invoices = invoices.Where(i => i.InvoiceType == invoiceType);

                Invoice_Details = await invoices.OrderBy(i => i.InvoicedOn).ToListAsync();

                TotalInvoiceValue = await invoices.SumAsync(i => i.InvoiceValue);
                TotalTaxValue = await invoices.SumAsync(i => i.TaxValue);

                await SetViewData();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
