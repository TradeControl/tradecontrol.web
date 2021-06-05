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

        public SelectList CashAccountNames { get; set; }
        [BindProperty(SupportsGet = true)]
        public string CashAccountName { get; set; }

        [BindProperty]
        public string CashAccountCode { get; set; }

        public IList<Cash_vwPaymentsUnposted> Cash_PaymentsUnposted { get;set; }

        public async Task OnGetAsync(string cashAccountCode, string cashAccountName)
        {
            try
            {
                var cashAccountNames = from t in NodeContext.Org_tbAccounts
                                       where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                                       orderby t.CashAccountName
                                       select t.CashAccountName;

                CashAccountNames = new SelectList(await cashAccountNames.ToListAsync());

                if (!string.IsNullOrEmpty(cashAccountName))
                {
                    CashAccountName = cashAccountName;
                    CashAccountCode = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountName == cashAccountName).Select(t => t.CashAccountCode).FirstOrDefaultAsync();
                }
                else if (!string.IsNullOrEmpty(cashAccountCode))
                    CashAccountCode = cashAccountCode;
                else if (CashAccountCode == null)
                    CashAccountCode = await NodeContext.CurrentAccount;

                if (string.IsNullOrEmpty(CashAccountName))
                    CashAccountName = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountCode == CashAccountCode).Select(t => t.CashAccountName).FirstOrDefaultAsync();

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
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                CashAccounts cashAccount = new(NodeContext);

                var profile = new Profile(NodeContext);
                var user = await UserManager.GetUserAsync(User);
                string userId = await profile.UserId(user.Id);

                if (await cashAccount.PostPayment(userId))
                    return RedirectToPage("./Index");
                else
                    throw new Exception($"Payment post failed for user {userId}");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
   }
}
