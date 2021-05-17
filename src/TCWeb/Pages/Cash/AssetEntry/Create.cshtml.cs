using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
namespace TradeControl.Web.Pages.Cash.AssetEntry
{
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        { }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_AssetsUnposted { get; set; }

        public SelectList CashAccountCodes { get; set; }


        public async Task<IActionResult> OnGetAsync(string cashAccountCode)
        {
            var cashAccountList = NodeContext.Org_CashAccountAssets.OrderBy(t => t.LiquidityLevel).Select(t => t.CashAccountCode);

            CashAccountCodes = new SelectList(await cashAccountList.ToListAsync());

            if (string.IsNullOrEmpty(cashAccountCode))
                cashAccountCode = await cashAccountList.FirstOrDefaultAsync();

            Profile profile = new(NodeContext);
            CashAccounts cashAccounts = new(NodeContext);

            var cashAccount = await NodeContext.Org_CashAccountAssets.Where(t => t.CashAccountCode == cashAccountCode).FirstAsync();

            Cash_AssetsUnposted = new Cash_vwPaymentsUnposted
            {
                CashAccountCode = cashAccount.CashAccountCode,
                PaymentCode = await cashAccounts.NextPaymentCode(),
                AccountCode = cashAccount.AccountCode,
                CashCode = cashAccount.CashCode,
                TaxCode = cashAccount.TaxCode,
                PaidOn = DateTime.Today,
                UserId = await profile.UserId(UserManager.GetUserId(User)),
                UpdatedBy = await profile.UserName(UserManager.GetUserId(User)),
                IsProfitAndLoss = true
            };

            Cash_AssetsUnposted.InsertedBy = Cash_AssetsUnposted.UpdatedBy;

            await SetViewData();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Cash_AssetsUnposted.UpdatedOn = DateTime.Now;
            Cash_AssetsUnposted.InsertedOn = DateTime.Now;

            if (!ModelState.IsValid || (Cash_AssetsUnposted.PaidInValue + Cash_AssetsUnposted.PaidOutValue == 0))
                return Page();

            NodeContext.Cash_PaymentsUnposted.Add(Cash_AssetsUnposted);
            await NodeContext.SaveChangesAsync();

            RouteValueDictionary route = new();
            route.Add("CashAccountCode", Cash_AssetsUnposted.CashAccountCode);

            return RedirectToPage("./Index", route);
        }
    }
}
