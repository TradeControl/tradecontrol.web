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

namespace TradeControl.Web.Pages.Org.CashAccount
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_tbAccount Org_CashAccount { get; set; }

        [BindProperty]
        public string OrganisationName { get; set; }
        public SelectList OrganisationNames { get; set; }

        [BindProperty]
        public string CashDescription { get; set; }
        public SelectList CashCodes { get; set; }


        [BindProperty]
        public string AccountType { get; set; }
        public SelectList AccountTypes { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountType)
        {
            try
            {
                AccountType = accountType;
                AccountTypes = new SelectList(await NodeContext.Org_tbAccountTypes.OrderBy(t => t.AccountTypeCode).Select(t => t.AccountType).ToListAsync());

                var accountNames = from tb in NodeContext.Org_AccountLookup
                                   orderby tb.AccountName
                                   select tb.AccountName;

                OrganisationNames = new SelectList(await accountNames.ToListAsync());
                OrganisationName = await NodeContext.App_HomeAccount.Select(t => t.AccountName).FirstAsync();

                var cashCodes = await (from tb in NodeContext.Cash_BankCashCodes
                                       orderby tb.CashDescription
                                       select tb.CashDescription).ToListAsync();

                cashCodes.Add(string.Empty);

                CashCodes = new SelectList(cashCodes);
                CashDescription = string.Empty;


                Profile profile = new(NodeContext);
                CashAccounts cashAccounts = new(NodeContext);
                NodeSettings settings = new(NodeContext);

                Org_CashAccount = new()
                {
                    AccountCode = await cashAccounts.CurrentAccount(),
                    CoinTypeCode = (short)await settings.CoinType,
                    AccountTypeCode = await NodeContext.Org_tbAccountTypes.Where(t => t.AccountType == AccountType).Select(t => t.AccountTypeCode).FirstOrDefaultAsync(),
                    LiquidityLevel = 0,
                    OpeningBalance = 0,
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User))
                };

                Org_CashAccount.UpdatedBy = Org_CashAccount.InsertedBy;

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
                Org_CashAccount.AccountTypeCode = await NodeContext.Org_tbAccountTypes.Where(t => t.AccountType == AccountType).Select(t => t.AccountTypeCode).FirstAsync();
                Org_CashAccount.AccountCode = await NodeContext.Org_tbOrgs.Where(t => t.AccountName == OrganisationName).Select(t => t.AccountCode).FirstAsync();

                if (!string.IsNullOrEmpty(CashDescription))
                    Org_CashAccount.CashCode = await NodeContext.Cash_tbCodes.Where(t => t.CashDescription == CashDescription).Select(t => t.CashCode).FirstAsync();

                Org_CashAccount.CurrentBalance = Org_CashAccount.OpeningBalance;

                if (!ModelState.IsValid)
                    return Page();

                NodeContext.Org_tbAccounts.Add(Org_CashAccount);
                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("AccountType", AccountType);

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
