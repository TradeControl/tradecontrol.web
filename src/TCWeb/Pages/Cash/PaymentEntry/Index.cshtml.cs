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
        public SelectList AccountNames { get; set; }
        [BindProperty(SupportsGet = true)]
        public string AccountName { get; set; }

        [BindProperty]
        public string AccountCode { get; set; }

        public IList<Cash_vwPayment> Cash_PaymentsUnposted { get;set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public IndexModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task OnGetAsync(string cashSubjectCode, string cashSubjectName)
        {
            try
            {
                var cashSubjectNames = from t in NodeContext.Subject_tbAccounts
                                       where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                                       orderby t.AccountName
                                       select t.AccountName;

                AccountNames = new SelectList(await cashSubjectNames.ToListAsync());

                if (!string.IsNullOrEmpty(cashSubjectName))
                {
                    AccountName = cashSubjectName;
                    AccountCode = await NodeContext.Subject_tbAccounts.Where(t => t.AccountName == cashSubjectName).Select(t => t.AccountCode).FirstOrDefaultAsync();
                }
                else if (!string.IsNullOrEmpty(cashSubjectCode))
                    AccountCode = cashSubjectCode;
                else
                    AccountCode ??= await NodeContext.CurrentAccount();

                if (string.IsNullOrEmpty(AccountName))
                    AccountName = await NodeContext.Subject_tbAccounts.Where(t => t.AccountCode == AccountCode).Select(t => t.AccountName).FirstOrDefaultAsync();

                var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                if (!isAuthorized)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    string userId = await profile.UserId(user.Id);

                    if (!string.IsNullOrEmpty(AccountCode))
                        Cash_PaymentsUnposted = await NodeContext.Cash_Payments
                                    .Where(t => t.AccountCode == AccountCode && t.UserId == userId && t.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Unposted)
                                    .ToListAsync();
                    else
                        Cash_PaymentsUnposted = await NodeContext.Cash_Payments
                                    .Where(t => t.UserId == userId && t.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Unposted)
                                    .ToListAsync();
                }
                else if (!string.IsNullOrEmpty(AccountCode))
                    Cash_PaymentsUnposted = await NodeContext.Cash_Payments
                                    .Where(t => t.AccountCode == AccountCode && t.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Unposted)
                                    .ToListAsync();
                else
                    Cash_PaymentsUnposted = await NodeContext.Cash_Payments
                                    .Where(t => t.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Unposted)
                                    .ToListAsync();

                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
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
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
   }
}
