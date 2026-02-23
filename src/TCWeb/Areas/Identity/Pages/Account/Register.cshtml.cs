using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<TradeControlWebUser> _signInManager;
        private readonly UserManager<TradeControlWebUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly NodeContext _nodeContext;

        public RegisterModel(
            UserManager<TradeControlWebUser> userManager,
            SignInManager<TradeControlWebUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            NodeContext nodeContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _nodeContext = nodeContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (!ViewData.ContainsKey("CompanyName"))
                ViewData.Add("CompanyName", await _nodeContext.CompanyName());
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            try
            {
                returnUrl ??= Url.Content("~/");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                if (!ModelState.IsValid)
                    return Page();

                var email = (Input.Email ?? string.Empty).Trim();

                var nodeSettings = new NodeSettings(_nodeContext);
                var isBootstrap = nodeSettings.IsFirstUse;

                if (!isBootstrap)
                {
                    if (!nodeSettings.HasMailHost)
                    {
                        ModelState.AddModelError(string.Empty, "Email service is not configured. Please contact an administrator.");
                        return Page();
                    }

                    var options = await _nodeContext.App_tbOptions.FirstOrDefaultAsync();
                    if (options?.UserRegistrationConfirmTemplateId == null || options?.UserRegistrationAdminNotifyTemplateId == null)
                    {
                        ModelState.AddModelError(string.Empty, "Registration templates are not configured. Please contact an administrator.");
                        return Page();
                    }
                }

                var user = new TradeControlWebUser { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return Page();
                }

                _logger.LogInformation("User created a new account with password.");

                if (isBootstrap)
                {
                    var code0 = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, code0);

                    var setupAdminEmail = await _nodeContext.Usr_tbUsers
                        .OrderBy(u => u.UserId)
                        .Select(u => u.EmailAddress)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(setupAdminEmail)
                        && string.Equals(setupAdminEmail.Trim(), email, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!await _userManager.IsInRoleAsync(user, Constants.AdministratorsRole))
                            await _userManager.AddToRoleAsync(user, Constants.AdministratorsRole);
                    }

                    return RedirectToPage("./Login", new { returnUrl });
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                if (_emailSender is IdentityEmailSender tcSender)
                {
                    await tcSender.SendRegistrationConfirmAsync(email, callbackUrl);
                }
                else
                {
                    await _emailSender.SendEmailAsync(email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                }

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    return RedirectToPage("RegisterConfirmation", new { email = email, returnUrl = returnUrl });

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            catch (Exception e)
            {
                await _nodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
