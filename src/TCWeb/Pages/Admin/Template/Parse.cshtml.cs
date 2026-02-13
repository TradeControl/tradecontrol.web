using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Pages.Admin.Template
{
    [Authorize(Roles = "Administrators")]
    public class ParseModel : DI_BasePageModel
    {
        private readonly IFileProvider _fileProvider;

        public ParseModel(NodeContext context, IFileProvider fileProvider) : base(context)
        {
            _fileProvider = fileProvider;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public async Task<IActionResult> OnPostInvoiceAsync(int templateId)
        {
            try
            {
                if (templateId <= 0)
                    return BadRequest("templateId must be specified.");

                TemplateManager templateManager = new(NodeContext, _fileProvider);
                var report = await templateManager.ParseTemplateAsync(templateId, MailInvoice.ParseProfile);

                return new JsonResult(report);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return StatusCode(500, e.Message);
            }
        }
    }
}
