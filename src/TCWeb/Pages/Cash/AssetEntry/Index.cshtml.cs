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
//select * from Subject.vwCashAccountAssets order by LiquidityLevel;
namespace TradeControl.Web.Pages.Cash.AssetEntry
{
    public class IndexModel : DI_BasePageModel
    {
        public SelectList CashAccounts { get; set; }

        [BindProperty(SupportsGet = true)]
        public string AccountName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string AccountCode { get; set; }

        public IList<Cash_vwPaymentsUnposted> Cash_AssetsUnposted { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public IndexModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task OnGetAsync(string cashSubjectName, string cashSubjectCode)
        {
            try
            {
                var cashAccounts = NodeContext.Subject_CashAccountAssets.Where(t => !t.AccountClosed).OrderBy(t => t.LiquidityLevel).Select(t => t.AccountName);

                CashAccounts = new SelectList(await cashAccounts.ToListAsync());

                if (!string.IsNullOrEmpty(cashSubjectName))
                    AccountName = cashSubjectName;
                else if (!string.IsNullOrEmpty(cashSubjectCode))
                    AccountName = await NodeContext.Subject_tbAccounts.Where(t => t.AccountCode == cashSubjectCode).Select(t => t.AccountName).FirstOrDefaultAsync();
                else if (await cashAccounts.AnyAsync())
                    AccountName = await cashAccounts.FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(cashSubjectCode))
                    AccountCode = await NodeContext.Subject_tbAccounts.Where(t => t.AccountName == AccountName).Select(t => t.AccountCode).FirstOrDefaultAsync();
                else
                    AccountCode = cashSubjectCode;

                var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                if (!isAuthorized)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    string userId = await profile.UserId(user.Id);

                    Cash_AssetsUnposted = await NodeContext.Cash_PaymentsUnposted.Where(t => t.AccountCode == AccountCode && t.UserId == userId).ToListAsync();
                }
                else
                    Cash_AssetsUnposted = await NodeContext.Cash_PaymentsUnposted.Where(t => t.AccountCode == AccountCode).ToListAsync();


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
