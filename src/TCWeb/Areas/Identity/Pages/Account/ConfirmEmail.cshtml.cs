using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<TradeControlWebUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly NodeContext _nodeContext;

        public ConfirmEmailModel(UserManager<TradeControlWebUser> userManager, IEmailSender emailSender, NodeContext nodeContext)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _nodeContext = nodeContext;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl = null)
        {
            try
            {
                if (userId == null || code == null)
                    return RedirectToPage("/Index");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound($"Unable to load user with ID '{userId}'.");

                var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

                StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";

                if (result.Succeeded)
                {
                    var adminUsers = await _userManager.GetUsersInRoleAsync(Constants.AdministratorsRole);
                    var adminEmails = adminUsers
                        .Select(u => u.Email)
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var usersUrl = $"{Request.Scheme}://{Request.Host}/Admin/Users/Index";

                    if (_emailSender is IdentityEmailSender tcSender)
                    {
                        await tcSender.SendRegistrationAdminNotifyAsync(adminEmails, user.Email, usersUrl);
                    }

                    if (!ViewData.ContainsKey("CompanyName"))
                        ViewData.Add("CompanyName", await _nodeContext.CompanyName());
                }

                if (!string.IsNullOrWhiteSpace(returnUrl))
                    return LocalRedirect(returnUrl);

                return Page();
            }
            catch (Exception e)
            {
                await _nodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
