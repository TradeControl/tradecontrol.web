using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using System;

namespace TradeControl.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<TradeControlWebUser> _userManager;
        private readonly IEmailSender _sender;
        private readonly NodeContext _nodeContext;

        public RegisterConfirmationModel(UserManager<TradeControlWebUser> userManager, IEmailSender sender, NodeContext nodeContext)
        {
            _userManager = userManager;
            _sender = sender;
            _nodeContext = nodeContext;
        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            try
            {
                if (email == null)
                    return RedirectToPage("/Index");

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return NotFound($"Unable to load user with email '{email}'.");

                if (!ViewData.ContainsKey("CompanyName"))
                    ViewData.Add("CompanyName", await _nodeContext.CompanyName);

                Email = email;
                // Once you add a real email sender, you should remove this code that lets you confirm the account
                DisplayConfirmAccountLink = true;
                if (DisplayConfirmAccountLink)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    EmailConfirmationUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);
                }

                return Page();
            }
            catch (Exception e)
            {
                _nodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
