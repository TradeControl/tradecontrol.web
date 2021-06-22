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
//select * from Org.vwCashAccountAssets order by LiquidityLevel;
namespace TradeControl.Web.Pages.Cash.AssetEntry
{
    public class IndexModel : DI_BasePageModel
    {
        public SelectList CashAccounts { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CashAccountName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CashAccountCode { get; set; }

        public IList<Cash_vwPaymentsUnposted> Cash_AssetsUnposted { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public IndexModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task OnGetAsync(string cashAccountName, string cashAccountCode)
        {
            try
            {
                var cashAccounts = NodeContext.Org_CashAccountAssets.Where(t => !t.AccountClosed).OrderBy(t => t.LiquidityLevel).Select(t => t.CashAccountName);

                CashAccounts = new SelectList(await cashAccounts.ToListAsync());

                if (!string.IsNullOrEmpty(cashAccountName))
                    CashAccountName = cashAccountName;
                else if (!string.IsNullOrEmpty(cashAccountCode))
                    CashAccountName = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountCode == cashAccountCode).Select(t => t.CashAccountName).FirstOrDefaultAsync();
                else if (await cashAccounts.AnyAsync())
                    CashAccountName = await cashAccounts.FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(cashAccountCode))
                    CashAccountCode = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountName == CashAccountName).Select(t => t.CashAccountCode).FirstOrDefaultAsync();
                else
                    CashAccountCode = cashAccountCode;

                var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                if (!isAuthorized)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    string userId = await profile.UserId(user.Id);

                    Cash_AssetsUnposted = await NodeContext.Cash_PaymentsUnposted.Where(t => t.CashAccountCode == CashAccountCode && t.UserId == userId).ToListAsync();
                }
                else
                    Cash_AssetsUnposted = await NodeContext.Cash_PaymentsUnposted.Where(t => t.CashAccountCode == CashAccountCode).ToListAsync();


                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }


        public IActionResult OnAccept()
        {
            return RedirectToPage("./Index");
        }
    }
}
