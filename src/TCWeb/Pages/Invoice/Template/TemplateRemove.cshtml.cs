using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Template
{
    [Authorize(Roles = "Administrators")]
    public class TemplateRemoveModel : DI_BasePageModel
    {
        [BindProperty]
        public Web_vwTemplateInvoice Web_TemplateInvoice { get; set; }

        public TemplateRemoveModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string invoiceType, int? templateId)
        {
            if (string.IsNullOrEmpty(invoiceType) || templateId == null)
                return NotFound();

            Web_TemplateInvoice = await NodeContext.Web_TemplateInvoices.FirstOrDefaultAsync(t => t.InvoiceType == invoiceType && t.TemplateId == templateId);

            if (Web_TemplateInvoice == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                TemplateManager templateManager = new TemplateManager(NodeContext);
                await templateManager.UnassignTemplateToInvoice((NodeEnum.InvoiceType)Web_TemplateInvoice.InvoiceTypeCode, Web_TemplateInvoice.TemplateId);

                RouteValueDictionary route = new();
                route.Add("InvoiceType", Web_TemplateInvoice.InvoiceType);

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
