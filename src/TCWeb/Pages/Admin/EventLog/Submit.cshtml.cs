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
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.EventLog
{
    [Authorize(Roles = "Administrators, Managers")]
    public class SubmitModel : DI_BasePageModel
    {
        public SubmitModel(NodeContext context) : base(context) { }

        [BindProperty]
        public App_vwEventLog App_EventLog { get; set; }

        public async Task<IActionResult> OnGetAsync(string logCode)
        {
            if (logCode == null)
                return NotFound();

            NodeSettings nodeSettings = new(NodeContext);

            if (!nodeSettings.HasMailHost)
                return RedirectToPage("/Admin/Host/Index");

            App_EventLog = await NodeContext.App_EventLogs.FirstOrDefaultAsync(e => e.LogCode == logCode);

            if (App_EventLog == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string logCode)
        {
            try
            {
                if (logCode == null)
                    return NotFound();
                
                var log = await NodeContext.App_EventLogs.FirstOrDefaultAsync(e => e.LogCode == logCode);

                TemplateManager templateManager = new(NodeContext);

                MailText mailText = await templateManager.GetText
                (
                    name: "Support",
                    emailTo: SupportRequest.SupportAddress,
                    subject: "trade control support",
                    body: $"{log.LogCode} {log.LoggedOn} {log.EventMessage}"
                );

                if (mailText != null)
                {
                    SupportRequest supportRequest = new(NodeContext, mailText);
                    await supportRequest.Send();
                }

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {                
                NodeContext.ErrorLog(e);
                throw;
            }




        }

    }
}
