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
    public class DeleteModel : DI_BasePageModel
    {
        UserManager<TradeControlWebUser> UserManager { get; }

        public DeleteModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
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
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string paymentCode)
        {
            try
            {
                if (paymentCode == null)
                    return NotFound();

                Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.FindAsync(paymentCode);

                if (Cash_PaymentsUnposted != null)
                {
                    NodeContext.Cash_PaymentsUnposted.Remove(Cash_PaymentsUnposted);
                    await NodeContext.SaveChangesAsync();
                }

                RouteValueDictionary route = new();
                route.Add("CashAccountCode", Cash_PaymentsUnposted.CashAccountCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
