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
    public class IndexModel : DI_BasePageModel
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

        public IList<Web_vwTemplateInvoice> Web_TemplateInvoices { get; set; }

        [BindProperty(SupportsGet = true)]
        public string InvoiceType { get; set; }
        public SelectList InvoiceTypes { get; set; }

        [BindProperty]
        [Display(Name = "Available Templates")]
        public string TemplateFileName { get; set; }
        public SelectList TemplateFileNames { get; set; }

        public IndexModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync(string invoiceType)
        {
            try
            {
                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());

                var templateInvoices = from tb in NodeContext.Web_TemplateInvoices select tb;

                if (!string.IsNullOrEmpty(invoiceType))
                    InvoiceType = invoiceType;
                else
                    InvoiceType = InvoiceTypes.First().Text;

                InvoiceTypeCode = await NodeContext.Invoice_tbTypes
                            .Where(t => t.InvoiceType == InvoiceType)
                            .Select(t => (NodeEnum.InvoiceType)t.InvoiceTypeCode)
                            .FirstAsync();

                templateInvoices = templateInvoices.Where(i => i.InvoiceType == InvoiceType);
                    
                Web_TemplateInvoices = await templateInvoices.OrderBy(i => i.LastUsedOn).ToListAsync();

                var templateFileNames = NodeContext.Web_tbTemplates
                                                    .OrderBy(t => t.TemplateFileName)
                                                    .Select(t => t.TemplateFileName)
                                                    .Except(NodeContext.Web_TemplateInvoices
                                                        .Where(i => i.InvoiceType == InvoiceType) 
                                                        .Select(i => i.TemplateFileName));

                TemplateFileNames = new SelectList(await templateFileNames.ToListAsync());
                if (TemplateFileNames.Any())
                    TemplateFileName = TemplateFileNames.First().Text;
                                                    
                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string templateFileName)
        {
            try
            {
                TemplateManager templateManager = new TemplateManager(NodeContext);

                await templateManager.AssignTemplateToInvoice(InvoiceTypeCode, templateFileName);

                RouteValueDictionary route = new();
                var invoiceType = await NodeContext.Invoice_tbTypes
                                        .Where(t => t.InvoiceTypeCode == (short)InvoiceTypeCode)
                                        .Select(t => t.InvoiceType)
                                        .SingleAsync();

                route.Add("InvoiceType", invoiceType);

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
