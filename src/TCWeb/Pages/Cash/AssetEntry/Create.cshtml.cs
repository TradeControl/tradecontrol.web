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
        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_AssetsUnposted { get; set; }

        public SelectList CashAccountNames { get; set; }
        [BindProperty] 
        public string CashAccountName { get; set; }

        public async Task<IActionResult> OnGetAsync(string cashAccountName)
        {
            try
            {
                var cashAccountList = NodeContext.Org_CashAccountAssets.Where(t => !t.AccountClosed).OrderBy(t => t.LiquidityLevel).Select(t => t.CashAccountName);

                CashAccountNames = new SelectList(await cashAccountList.ToListAsync());

                if (string.IsNullOrEmpty(cashAccountName))
                    cashAccountName = await cashAccountList.FirstOrDefaultAsync();

                Profile profile = new(NodeContext);
                CashAccounts cashAccounts = new(NodeContext);

                var cashAccount = await NodeContext.Org_CashAccountAssets.Where(t => t.CashAccountName == cashAccountName).FirstAsync();

                Cash_AssetsUnposted = new Cash_vwPaymentsUnposted
                {
                    CashAccountCode = cashAccount.CashAccountCode,
                    PaymentCode = await cashAccounts.NextPaymentCode(),
                    AccountCode = cashAccount.AccountCode,
                    CashCode = cashAccount.CashCode,
                    TaxCode = cashAccount.TaxCode,
                    PaidOn = DateTime.Today,
                    UserId = await profile.UserId(UserManager.GetUserId(User)),
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User)),
                    IsProfitAndLoss = true
                };

                Cash_AssetsUnposted.UpdatedBy = Cash_AssetsUnposted.InsertedBy;

                await SetViewData();

                return Page();
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
                Cash_AssetsUnposted.CashAccountCode = await NodeContext.Org_CashAccountAssets
                                                    .Where(t => t.CashAccountName == CashAccountName)
                                                    .Select(t => t.CashAccountCode)
                                                    .SingleAsync();

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
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
