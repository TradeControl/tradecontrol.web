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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
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
        [BindProperty]
        public App_vwEventLog App_EventLog { get; set; }

        [BindProperty]
        [Display(Name = "Additional Info.")]
        public string Note { get; set; } = string.Empty;

        private IConfiguration Configuration { get; }
        private IFileProvider FileProvider { get; }

        public SubmitModel(NodeContext context, IConfiguration configuration, IFileProvider fileProvider) : base(context)
        {
            Configuration = configuration;
            FileProvider = fileProvider;
        }

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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                TemplateManager templateManager = new(NodeContext, FileProvider);

                string emailAddress = Configuration.GetSection("Settings")["SupportEmailAddress"];

                MailDocument doc = await templateManager.GetSupportRequest();
                MailSupport mailSupport = new(NodeContext, doc, App_EventLog.LogCode);

                if (!string.IsNullOrEmpty(Note))
                    await mailSupport.Send(emailAddress, Note);
                else
                    await mailSupport.Send(emailAddress);

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {                
                await NodeContext.ErrorLog(e);
                throw;
            }




        }

    }
}
