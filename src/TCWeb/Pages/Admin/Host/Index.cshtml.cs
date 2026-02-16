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
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Host
{
    [Authorize(Roles = "Administrators")]
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        UserManager<TradeControlWebUser> UserManager { get; }

        public IList<App_tbHost> App_Hosts { get; set; }

        public SelectList HostDescriptions { get; set; }

        [BindProperty]
        [Display(Name = "Active Host")]
        public string HostDescription { get; set; }

        public string? StatusMessage { get; set; }
        public bool StatusOk { get; set; }

        [TempData]
        public string? FlashMessage { get; set; }

        [TempData]
        public bool FlashOk { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                App_Hosts = await NodeContext.App_tbHosts.OrderBy(h => h.HostDescription).ToListAsync();
                HostDescriptions = new SelectList(await NodeContext.App_tbHosts.OrderBy(h => h.HostDescription).Select(h => h.HostDescription).ToListAsync());
                HostDescription = await NodeContext.App_Host.Select(h => h.HostDescription).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                int hostId = await NodeContext.App_tbHosts.Where(h => h.HostDescription == HostDescription).Select(h => h.HostId).FirstAsync();

                NodeSettings settings = new(NodeContext);

                if (await settings.SetHost(hostId))
                    return RedirectToPage("./Index");
                else
                    return RedirectToPage("/Admin/EventLog/Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostTestAsync()
        {
            const string handlerError = "Test email failed";

            try
            {
                var embedded = Request.Query.TryGetValue("embedded", out var emb) && emb == "1";
                var returnNode = Request.Query.TryGetValue("returnNode", out var rn) ? rn.ToString() : "Host";

                var redirectUrl = embedded
                    ? $"/Admin/Host/Index?embedded=1&returnNode={Uri.EscapeDataString(returnNode)}"
                    : $"/Admin/Host/Index?returnNode={Uri.EscapeDataString(returnNode)}";

                var user = await UserManager.GetUserAsync(User);
                var to = user?.Email;

                if (string.IsNullOrWhiteSpace(to))
                {
                    await NodeContext.EventLog(NodeEnum.EventType.IsInformation,
                        "Test email NOT sent: current user has no email address configured.");

                    FlashOk = false;
                    FlashMessage = "Test email not sent. Check Event Logs for details.";

                    return Redirect(redirectUrl);
                }

                var settings = new NodeSettings(NodeContext);
                var mailSettings = await settings.MailHost();

                if (mailSettings == null)
                {
                    await NodeContext.EventLog(NodeEnum.EventType.IsInformation,
                        "Test email NOT sent: no active mail host is configured.");

                    FlashOk = false;
                    FlashMessage = "Test email not sent. Check Event Logs for details.";

                    return Redirect(redirectUrl);
                }

                var from = mailSettings.UserName;
                var subject = "Trade Control - Test Email";
                var body = $"Test email sent at {DateTime.Now:G}.";

                await NodeContext.EventLog(NodeEnum.EventType.IsInformation,
                    $"Test email requested. From='{from}', To='{to}', Subject='{subject}', Body='{body}'.");

                var mail = new MailText {
                    Settings = mailSettings,
                    Name = user?.UserName ?? "Administrator",
                    EmailTo = to,
                    Subject = subject,
                    Body = body,
                    IsHtml = false
                };

                var sender = new TestMailSender();
                await sender.SendAsync(mail);

                await NodeContext.EventLog(NodeEnum.EventType.IsInformation,
                    $"Test email sent successfully. From='{from}', To='{to}', Subject='{subject}'.");

                FlashOk = true;
                FlashMessage = "Email sent. Check Event Logs for details.";

                return Redirect(redirectUrl);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);

                try
                {
                    await NodeContext.EventLog(NodeEnum.EventType.IsError,
                        $"{handlerError}: {e.GetType().Name}: {e.Message}");
                }
                catch
                {
                }

                var returnNode = Request.Query.TryGetValue("returnNode", out var rn) ? rn.ToString() : "Host";
                var embedded = Request.Query.TryGetValue("embedded", out var emb) && emb == "1";

                var redirectUrl = embedded
                    ? $"/Admin/Host/Index?embedded=1&returnNode={Uri.EscapeDataString(returnNode)}"
                    : $"/Admin/Host/Index?returnNode={Uri.EscapeDataString(returnNode)}";

                FlashOk = false;
                FlashMessage = "Test email failed. Check Event Logs for details.";

                return Redirect(redirectUrl);
            }
        }
    }
}
