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
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Org.CashAccount
{
    [Authorize(Roles = "Administrators")]
    public class IndexModel : DI_BasePageModel
    {
        [BindProperty]
        public IList<Org_vwCashAccount> Org_CashAccounts { get; set; }

        [BindProperty]
        public string AccountType { get; set; }

        public SelectList AccountTypes { get; set; }

        public IndexModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync(string accountType, string cashAccountCode)
        {
            try
            {

                AccountTypes = new SelectList(await NodeContext.Org_tbAccountTypes.OrderBy(t => t.AccountTypeCode).Select(t => t.AccountType).ToListAsync());

                if (!string.IsNullOrEmpty(cashAccountCode))
                    AccountType = await NodeContext.Org_CashAccounts.Where(t => t.CashAccountCode == cashAccountCode).Select(t => t.AccountType).FirstOrDefaultAsync();
                else if (string.IsNullOrEmpty(accountType))
                    AccountType = await NodeContext.Org_tbAccountTypes.Where(t => t.AccountTypeCode == (short)NodeEnum.CashAccountType.Cash).Select(t => t.AccountType).FirstAsync();
                else
                    AccountType = accountType;

                var cashAccounts = from tb in NodeContext.Org_CashAccounts
                                   where tb.AccountType == AccountType
                                   orderby tb.AccountTypeCode, tb.LiquidityLevel
                                   select tb;

                Org_CashAccounts = await cashAccounts.ToListAsync();

                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
