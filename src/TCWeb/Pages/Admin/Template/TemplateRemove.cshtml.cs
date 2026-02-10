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

namespace TradeControl.Web.Pages.Admin.Template
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

            Web_TemplateInvoice = await NodeContext.Web_TemplateInvoices
                .FirstOrDefaultAsync(t => t.InvoiceType == invoiceType && t.TemplateId == templateId);

            if (Web_TemplateInvoice == null)
                return NotFound();

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? "Templates")
                    : "Templates";

                TemplateManager templateManager = new TemplateManager(NodeContext);
                await templateManager.UnassignTemplateToInvoice(
                    (NodeEnum.InvoiceType)Web_TemplateInvoice.InvoiceTypeCode,
                    Web_TemplateInvoice.TemplateId);

                // Redirect back to the invoice template list, preserving embedded context
                var embeddedQs = embedded ? "embedded=1&" : string.Empty;

                return Redirect($"/Admin/Template/Invoices?{embeddedQs}returnNode={Uri.EscapeDataString(returnNode)}&invoiceType={Uri.EscapeDataString(Web_TemplateInvoice.InvoiceType)}");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
