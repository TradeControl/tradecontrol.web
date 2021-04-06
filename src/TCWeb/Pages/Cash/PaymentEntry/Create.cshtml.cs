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
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }
        public int InputMode { get; set; }

        public SelectList CashAccountCodes { get; set; }
        public SelectList AccountCodes { get; set; }
        public SelectList CashCodes { get; set; }
        public SelectList TaxCodes { get; set; }

        public async Task<IActionResult> OnGetAsync(string id, string mode)
        {

            var cashAccountCodes = from t in NodeContext.Org_tbAccounts
                               where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                               orderby t.CashAccountCode
                               select t.CashAccountCode;

            CashAccountCodes = new SelectList(await cashAccountCodes.ToListAsync());

            var accountCodes = from t in NodeContext.Org_AccountLookup
                              orderby t.AccountCode
                              select t.AccountCode;

            AccountCodes = new SelectList(await accountCodes.ToListAsync());

            if (mode != "0")
            {
                InputMode = 1;
                var cashCodes = from t in NodeContext.Cash_CodeLookup
                                where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                orderby t.CashCode
                                select t.CashCode;

                CashCodes = new SelectList(await cashCodes.ToListAsync());

                var taxCodes = from t in NodeContext.App_TaxCodes
                               orderby t.TaxCode
                               select t.TaxCode;

                TaxCodes = new SelectList(await taxCodes.ToListAsync());
            }
            else
                InputMode = 0;


            Cash_PaymentsUnposted = new Cash_vwPaymentsUnposted();
            Cash_PaymentsUnposted.CashAccountCode = id;
            Cash_PaymentsUnposted.PaidOn = DateTime.Today;

            Profile profile = new Profile(NodeContext);
            Cash_PaymentsUnposted.UserId = await profile.UserId(UserManager.GetUserId(User));
            Cash_PaymentsUnposted.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));
            Cash_PaymentsUnposted.InsertedBy = Cash_PaymentsUnposted.UpdatedBy;

            Cash_PaymentsUnposted.IsProfitAndLoss = false;

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CashAccounts cashAccounts = new CashAccounts(NodeContext);
            Cash_PaymentsUnposted.PaymentCode = await cashAccounts.NextPaymentCode();

            Cash_PaymentsUnposted.UpdatedOn = DateTime.Now;
            Cash_PaymentsUnposted.InsertedOn = DateTime.Now;

            if (!ModelState.IsValid)
                return Page();

            NodeContext.Cash_PaymentsUnposted.Add(Cash_PaymentsUnposted);
            await NodeContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
