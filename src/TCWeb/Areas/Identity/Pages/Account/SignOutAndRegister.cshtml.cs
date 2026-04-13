using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradeControl.Web.Areas.Identity.Data;

namespace TradeControl.Web.Pages.Account
{
    [AllowAnonymous]
    public class SignOutAndRegisterModel : PageModel
    {
        private readonly SignInManager<TradeControlWebUser> _signInManager;

        public SignOutAndRegisterModel(SignInManager<TradeControlWebUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task OnGetAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
