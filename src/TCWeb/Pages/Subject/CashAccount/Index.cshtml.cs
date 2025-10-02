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

namespace TradeControl.Web.Pages.Subject.CashAccount
{
    [Authorize(Roles = "Administrators")]
    public class IndexModel : DI_BasePageModel
    {
        [BindProperty]
        public IList<Subject_vwCashAccount> Subject_CashAccounts { get; set; }

        [BindProperty]
        public string AccountType { get; set; }

        public SelectList AccountTypes { get; set; }

        public IndexModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync(string accountType, string cashSubjectCode)
        {
            try
            {

                AccountTypes = new SelectList(await NodeContext.Subject_tbAccountTypes.OrderBy(t => t.AccountTypeCode).Select(t => t.AccountType).ToListAsync());

                if (!string.IsNullOrEmpty(cashSubjectCode))
                    AccountType = await NodeContext.Subject_CashAccounts.Where(t => t.AccountCode == cashSubjectCode).Select(t => t.AccountType).FirstOrDefaultAsync();
                else if (string.IsNullOrEmpty(accountType))
                    AccountType = await NodeContext.Subject_tbAccountTypes.Where(t => t.AccountTypeCode == (short)NodeEnum.CashAccountType.Cash).Select(t => t.AccountType).FirstAsync();
                else
                    AccountType = accountType;

                var cashAccounts = from tb in NodeContext.Subject_CashAccounts
                                   where tb.AccountType == AccountType
                                   orderby tb.AccountTypeCode, tb.LiquidityLevel
                                   select tb;

                Subject_CashAccounts = await cashAccounts.ToListAsync();

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
