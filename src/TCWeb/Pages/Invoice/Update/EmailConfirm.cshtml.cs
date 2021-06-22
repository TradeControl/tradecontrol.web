using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class EmailConfirmModel : DI_BasePageModel
    {
        private IFileProvider FileProvider { get; }
        UserManager<TradeControlWebUser> UserManager { get; }

        [BindProperty(SupportsGet = true)]
        public Invoice_vwRegister Invoice_Header { get; set; }

        [BindProperty]
        [Display(Name = "Select Template:")]
        public string TemplateFileName { get; set; }
        public SelectList TemplateFileNames { get; set; }

        [BindProperty]
        [Display(Name = "Select Recipient:")]
        public string EmailAddress { get; set; }
        public SelectList EmailAddresses { get; set; }

        public EmailConfirmModel(NodeContext context, UserManager<TradeControlWebUser> userManager, IFileProvider fileProvider) : base(context)
        {
            FileProvider = fileProvider;
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber, string emailAddress)
        {
            try
            {
                if (invoiceNumber == null)
                    return NotFound();

                Invoice_Header = await NodeContext.Invoice_Register.FirstOrDefaultAsync(m => m.InvoiceNumber == invoiceNumber);

                if (Invoice_Header == null)
                    return NotFound();
                else
                {
                    var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                    if (!isAuthorized)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        string userId = await profile.UserId(user.Id);

                        if (userId != Invoice_Header.UserId)
                            return Forbid();
                    }

                    var defaultTemplate = from i in NodeContext.Web_tbTemplateInvoices
                                          join t in NodeContext.Web_tbTemplates on i.TemplateId equals t.TemplateId
                                          where i.InvoiceTypeCode == Invoice_Header.InvoiceTypeCode
                                          orderby i.LastUsedOn descending
                                          select t.TemplateFileName;


                    TemplateFileNames = new SelectList(await defaultTemplate.ToListAsync());

                    if (TemplateFileNames.Any())
                        TemplateFileName = TemplateFileNames.First().Text;

                    var emailAddresses = from tb in NodeContext.Org_EmailAddresses
                                         where tb.AccountCode == Invoice_Header.AccountCode
                                         orderby tb.EmailAddress
                                         select tb;

                    EmailAddresses = new SelectList(await emailAddresses.Select(t => t.EmailAddress).Distinct().ToListAsync());

                    if (EmailAddresses.Any())
                    {
                        if (!string.IsNullOrEmpty(emailAddress) && await emailAddresses.Where(t => t.EmailAddress == emailAddress).AnyAsync())
                            EmailAddress = emailAddress;
                        else if (await emailAddresses.Where(t => t.IsAdmin).AnyAsync())
                            EmailAddress = await emailAddresses.Where(t => t.IsAdmin).Select(t => t.EmailAddress).SingleAsync();
                        else
                            EmailAddress = EmailAddresses.First().Text;
                    }

                    await SetViewData();
                    return Page();
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostPreviewDocument()
        {
            try
            {
                if (string.IsNullOrEmpty(TemplateFileName) || string.IsNullOrEmpty(EmailAddress))
                    return RedirectToPage("./Index");
                else
                {
                    int templateId = await NodeContext.Web_tbTemplates
                                    .Where(t => t.TemplateFileName == TemplateFileName)
                                    .Select(t => t.TemplateId)
                                    .FirstAsync();

                    RouteValueDictionary route = new();
                    route.Add("invoiceNumber", Invoice_Header.InvoiceNumber);
                    route.Add("templateId", templateId);

                    return RedirectToPage("./EmailPreview", route);
                }

            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostSubmitDocument()
        {
            try
            {
                if (string.IsNullOrEmpty(TemplateFileName) || string.IsNullOrEmpty(EmailAddress))
                    return RedirectToPage("./Index");
                else
                {
                    int templateId = await NodeContext.Web_tbTemplates
                                    .Where(t => t.TemplateFileName == TemplateFileName)
                                    .Select(t => t.TemplateId)
                                    .FirstAsync();

                    TemplateManager templateManager = new(NodeContext, FileProvider);

                    var invoice = await NodeContext.Invoice_tbInvoices.Where(i => i.InvoiceNumber == Invoice_Header.InvoiceNumber).SingleAsync();

                    MailDocument doc = await templateManager.GetInvoice((NodeEnum.InvoiceType)invoice.InvoiceTypeCode, (int)templateId);
                    MailInvoice mailInvoice = new(NodeContext, doc, invoice.InvoiceNumber);
                    await mailInvoice.Send(EmailAddress);
                    await templateManager.RegisterTemplateUsage(templateId, (NodeEnum.InvoiceType)invoice.InvoiceTypeCode);

                    return RedirectToPage("./Index");
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }

}