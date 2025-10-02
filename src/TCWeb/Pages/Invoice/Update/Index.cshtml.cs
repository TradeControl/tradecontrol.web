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

        public async Task OnGetAsync(string accountCode, bool printed, string invoiceNumber)
        {
            try
            {
                var invoices = from tb in NodeContext.Invoice_Register select tb;

                if (!string.IsNullOrEmpty(accountCode))
                    invoices = invoices.Where(i => i.SubjectCode == accountCode);
                else if (!string.IsNullOrEmpty(invoiceNumber))
                    invoices = invoices.Where(i => i.InvoiceNumber == invoiceNumber);
                else if (!printed)
                    invoices = invoices.Where(i => !i.Printed);
                else
                    invoices = invoices.OrderBy(i => i.InvoicedOn).Take(100);

                Invoice_Headers = await invoices.OrderByDescending(i => i.InvoicedOn).ToListAsync();

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
