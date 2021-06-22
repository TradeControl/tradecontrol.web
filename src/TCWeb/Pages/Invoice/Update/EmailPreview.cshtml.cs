using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class EmailPreviewModel : DI_BasePageModel
    {
        private readonly IFileProvider FileProvider;

        public EmailPreviewModel(
            NodeContext context,
            
            
            IFileProvider fileProvider) : base(context)
        {
            FileProvider = fileProvider;
        }

        [BindProperty]
        public string DocumentHtml { get; set; }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber, int? templateId)
        {
            try
            {
                if (invoiceNumber == null || templateId == null)
                    return NotFound();

                TemplateManager templateManager = new(NodeContext, FileProvider); 
                
                var invoice = await NodeContext.Invoice_tbInvoices.Where(i => i.InvoiceNumber == invoiceNumber).SingleOrDefaultAsync();

                if (invoice == null)
                    return NotFound();

                if (await NodeContext.Web_tbTemplates.Where(t => t.TemplateId == (int)templateId).AnyAsync())
                {
                    MailDocument doc = await templateManager.GetInvoice((NodeEnum.InvoiceType)invoice.InvoiceTypeCode, (int)templateId);
                    MailInvoice mailInvoice = new(NodeContext, doc, invoiceNumber);
                    
                    DocumentHtml = await mailInvoice.PreviewInvoice();

                    await templateManager.RegisterTemplateUsage((int)templateId, (NodeEnum.InvoiceType)invoice.InvoiceTypeCode);

                    await SetViewData();
                    return Page();
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
    
}
