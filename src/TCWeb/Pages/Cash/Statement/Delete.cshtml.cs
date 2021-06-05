using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;


namespace TradeControl.Web.Pages.Cash.Statement
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        [BindProperty]
        public Cash_tbPayment Cash_Payment { get; set; }

        public DeleteModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string paymentCode)
        {
            try
            {
                if (paymentCode == null)
                    return NotFound();

                Cash_Payment = await NodeContext.Cash_tbPayments.FirstOrDefaultAsync(m => m.PaymentCode == paymentCode);

                if (Cash_Payment == null)
                    return NotFound();
                else
                {
                    if ((User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole)) == false)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        if (Cash_Payment.UserId != await profile.UserId(user.Id))
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

                Cash_Payment = await NodeContext.Cash_tbPayments.FindAsync(paymentCode);

                if (Cash_Payment != null)
                {
                    CashAccounts cashAccounts = new(NodeContext);
                    if (await cashAccounts.DeletePayment(Cash_Payment.PaymentCode))
                        return RedirectToPage("./Index");
                    else
                        return Page();
                }
                else
                    return RedirectToPage("./Index");

            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
