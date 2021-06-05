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

namespace TradeControl.Web.Pages.Cash.AssetEntry
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
        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

        public async Task<IActionResult> OnGetAsync(string paymentCode)
        {
            try
            {
                if (paymentCode == null)
                    return NotFound();

                Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.FirstOrDefaultAsync(m => m.PaymentCode == paymentCode);

                if (Cash_PaymentsUnposted == null)
                    return NotFound();
                else
                {
                    if ((User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole)) == false)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        if (Cash_PaymentsUnposted.UserId != await profile.UserId(user.Id))
                            return Forbid();
                    }

                    await SetViewData();
                    return Page();
                }
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string paymentCode)
        {
            try
            {
                if (paymentCode == null)
                    return NotFound();

                CashAccounts cashAccounts = new(NodeContext);
                if (await cashAccounts.PostAsset(paymentCode))
                {
                    await NodeContext.SaveChangesAsync();

                    string cashAccountCode = await NodeContext.Cash_tbPayments.Where(t => t.PaymentCode == paymentCode).Select(t => t.CashAccountCode).FirstAsync();

                    RouteValueDictionary route = new();
                    route.Add("CashAccountCode", cashAccountCode);

                    return RedirectToPage("./Index", route);
                }
                else
                    throw new Exception("Post failed! Consult logs");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
