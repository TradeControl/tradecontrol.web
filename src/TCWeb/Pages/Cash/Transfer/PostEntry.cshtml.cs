using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Transfer
{
    public class PostEntryModel : DI_BasePageModel
    {
        public PostEntryModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public Cash_vwTransfersUnposted Cash_TransfersUnposted { get; set; }

        public async Task<IActionResult> OnGetAsync(string paymentCode)
        {
            if (paymentCode == null)
                return NotFound();

            Cash_TransfersUnposted = await NodeContext.Cash_TransfersUnposted.FirstOrDefaultAsync(m => m.PaymentCode == paymentCode);

            if (Cash_TransfersUnposted == null)
                return NotFound();
            else
            {
                if ((User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole)) == false)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    if (Cash_TransfersUnposted.UserId != await profile.UserId(user.Id))
                        return Forbid();
                }

                await SetViewData();
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAsync(string paymentCode)
        {
            if (paymentCode == null)
                return NotFound();

            CashAccounts cashAccounts = new (NodeContext);
            if (await cashAccounts.PostTransfer(paymentCode))
                return RedirectToPage("./Index");
            else
                throw new Exception("Transfer failed! Consult logs");
        }
    }
}
