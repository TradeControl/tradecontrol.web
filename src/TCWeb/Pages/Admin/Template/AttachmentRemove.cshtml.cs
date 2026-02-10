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
    public class AttachmentRemoveModel : DI_BasePageModel
    {
        [BindProperty]
        public string InvoiceType { get; set; }

        [BindProperty]
        public Web_vwAttachmentInvoice Web_AttachmentInvoice { get; set; }

        public AttachmentRemoveModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string invoiceType, int? attachmentId)
        {
            if (string.IsNullOrEmpty(invoiceType) || attachmentId == null)
                return NotFound();

            InvoiceType = invoiceType;
            Web_AttachmentInvoice = await NodeContext.Web_AttachmentInvoices.FirstOrDefaultAsync(t => t.InvoiceType == invoiceType && t.AttachmentId == attachmentId);            

            if (Web_AttachmentInvoice == null)
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

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? "Templates")
                    : "Templates";

                TemplateManager templateManager = new TemplateManager(NodeContext);
                NodeEnum.InvoiceType invoiceType = (NodeEnum.InvoiceType)await NodeContext.Invoice_tbTypes
                    .Where(t => t.InvoiceType == InvoiceType)
                    .Select(t => t.InvoiceTypeCode)
                    .SingleAsync();

                await templateManager.UnassignAttatchmentToInvoice(invoiceType, Web_AttachmentInvoice.AttachmentId);

                var embeddedQs = embedded ? "embedded=1&" : string.Empty;

                return Redirect($"/Admin/Template/Attachments?{embeddedQs}returnNode={Uri.EscapeDataString(returnNode)}&invoiceType={Uri.EscapeDataString(InvoiceType)}");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
