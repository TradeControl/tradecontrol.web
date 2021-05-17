using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public SelectList CashAccounts { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CashAccountCode { get; set; }

        public IList<Cash_vwPaymentsUnposted> Cash_PaymentsUnposted { get;set; }

        public async Task OnGetAsync(string cashAccountCode)
        {
            if (!string.IsNullOrEmpty(cashAccountCode))
                CashAccountCode = cashAccountCode;
            else if (CashAccountCode == null)
                CashAccountCode = await NodeContext.CurrentAccount;

            var cashAccounts = from t in NodeContext.Org_tbAccounts
                               where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                               orderby t.CashAccountCode
                               select t.CashAccountCode;

            CashAccounts = new SelectList(await cashAccounts.ToListAsync());

            var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);            

            if (!isAuthorized)
            {
                var profile = new Profile(NodeContext);
                var user = await UserManager.GetUserAsync(User);
                string userId = await profile.UserId(user.Id);

                if (!string.IsNullOrEmpty(CashAccountCode))
                    Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.Where(t => t.CashAccountCode == CashAccountCode && t.UserId == userId).ToListAsync();
                else
                    Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.Where(t => t.UserId == userId).Select(t => t).ToListAsync();
            }
            else if (!string.IsNullOrEmpty(CashAccountCode))
                Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.Where(t => t.CashAccountCode == CashAccountCode).ToListAsync();
            else
                Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.Select(t => t).ToListAsync();

            await SetViewData();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            CashAccounts cashAccount = new (NodeContext);

            var profile = new Profile(NodeContext);
            var user = await UserManager.GetUserAsync(User);
            string userId = await profile.UserId(user.Id);

            if (await cashAccount.PostPayment(userId))
                return RedirectToPage("../../Index");
            else
                return RedirectToPage("../../Index");  //Error Log Page
        }
   }
}
