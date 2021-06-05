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
        public UnpaidModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

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

        public async Task OnGetAsync(string invoiceType)
        {
            try
            {
                await SetViewData();

                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());

                var invoices = from tb in NodeContext.Invoice_RegisterOverdue select tb;

                if (!string.IsNullOrEmpty(invoiceType))
                    invoices = invoices.Where(i => i.InvoiceType == invoiceType);

                Invoice_RegisterOverdue = await invoices.OrderBy(i => i.ExpectedOn).ToListAsync();

                TotalInvoiceValue = (double)await invoices.SumAsync(i => i.InvoiceValue + i.TaxValue);
                TotalPaidValue = (double)await invoices.SumAsync(i => i.UnpaidValue);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
