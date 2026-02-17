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
                    ViewData.Add("CompanyName", await _nodeContext.CompanyName());

                Email = email;
                DisplayConfirmAccountLink = false;
                EmailConfirmationUrl = string.Empty;

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
