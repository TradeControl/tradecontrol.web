using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Template
{
    [Authorize(Roles = "Administrators")]
    public class AttachmentsModel : DI_BasePageModel
    {
        const string SessionKeyInvoiceType = "_InvoiceType";

        public NodeEnum.InvoiceType InvoiceTypeCode
        {
            get
            {
                try
                {
                    var invoiceType = HttpContext.Session.GetInt32(SessionKeyInvoiceType);
                    return (NodeEnum.InvoiceType)invoiceType;
                }
                catch
                {
                    return NodeEnum.InvoiceType.SalesInvoice;
                }
            }
            set
            {
                int invoiceType = (int)value;
                HttpContext.Session.SetInt32(SessionKeyInvoiceType, invoiceType);
            }
        }

        public string InvoiceType {get; set; }

        public IList<Web_vwAttachmentInvoice> Web_AttachmentInvoices { get; set; }

        [BindProperty]
        [Display(Name = "Available Documents")]
        public string AttachmentFileName { get; set; }
        public SelectList AttachmentFileNames { get; set; }

        public AttachmentsModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string invoiceType)
        {
            try
            {
                if (string.IsNullOrEmpty(invoiceType))
                    return NotFound();

                InvoiceTypeCode = (NodeEnum.InvoiceType)await NodeContext.Invoice_tbTypes
                                    .Where(t => t.InvoiceType == invoiceType)
                                    .Select(t => t.InvoiceTypeCode)
                                    .SingleAsync();

                InvoiceType = invoiceType;

                Web_AttachmentInvoices = await NodeContext.Web_AttachmentInvoices
                                    .Where(a => a.InvoiceType == invoiceType)
                                    .OrderBy(a => a.AttachmentFileName).ToListAsync();

                var attachmentFileNames = NodeContext.Web_tbAttachments
                                    .OrderBy(t => t.AttachmentFileName)
                                    .Select(t => t.AttachmentFileName)
                                    .Except(NodeContext.Web_AttachmentInvoices
                                        .Where(i => i.InvoiceType == InvoiceType)
                                        .Select(i => i.AttachmentFileName));

                AttachmentFileNames = new SelectList(await attachmentFileNames.ToListAsync());
                if (AttachmentFileNames.Any())
                    AttachmentFileName = AttachmentFileNames.First().Text;

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string attachmentFileName)
        {
            try
            {
                TemplateManager templateManager = new TemplateManager(NodeContext);

                await templateManager.AssignAttatchmentToInvoice(InvoiceTypeCode, attachmentFileName);

                RouteValueDictionary route = new();
                var invoiceType = await NodeContext.Invoice_tbTypes
                                        .Where(t => t.InvoiceTypeCode == (short)InvoiceTypeCode)
                                        .Select(t => t.InvoiceType)
                                        .SingleAsync();

                route.Add("InvoiceType", invoiceType);

                return RedirectToPage("./Attachments", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
