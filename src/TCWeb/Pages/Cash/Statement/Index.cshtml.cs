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

namespace TradeControl.Web.Pages.Cash.Statement
{
    public class IndexModel : DI_BasePageModel
    {

        [BindProperty]
        public string CashAccountName { get; set; }
        public SelectList CashAccountNames { get; set; }

        [BindProperty]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public IList<Cash_vwAccountStatement> Cash_AccountStatement { get; set; }

        [BindProperty]
        public Org_vwCashAccount Org_CashAccount { get; set; }

        public IndexModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync(string cashAccountCode, string cashAccountName, string periodName, string paymentCode, short mode)
        {
            try
            {
                await SetViewData();

                DateTime startOn = DateTime.Today;

                var accounts = from tb in NodeContext.Org_tbAccounts
                               where !tb.AccountClosed
                               orderby tb.AccountTypeCode, tb.CashAccountName
                               select tb.CashAccountName;

                CashAccountNames = new SelectList(await accounts.ToListAsync());

                var periodNames = from tb in NodeContext.App_Periods
                                  where tb.CashStatusCode == (short)NodeEnum.CashStatus.Current || tb.CashStatusCode == (short)NodeEnum.CashStatus.Closed
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());


                if (!string.IsNullOrEmpty(paymentCode))
                {
                    var payment = await NodeContext.Cash_AccountStatements.Where(t => t.PaymentCode == paymentCode).FirstOrDefaultAsync();

                    if (payment != null)
                    {
                        cashAccountCode = payment.CashAccountCode;
                        startOn = payment.StartOn;
                        periodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                        cashAccountName = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountCode == cashAccountCode).Select(t => t.CashAccountName).FirstOrDefaultAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(cashAccountCode))
                {
                    cashAccountName = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountCode == cashAccountCode).Select(t => t.CashAccountName).FirstOrDefaultAsync();
                }
                else if (string.IsNullOrEmpty(cashAccountName))
                {
                    CashAccounts cashAccounts = new(NodeContext);
                    cashAccountCode = await cashAccounts.CurrentAccount();
                    cashAccountName = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountCode == cashAccountCode).Select(t => t.CashAccountName).FirstOrDefaultAsync();
                }
                else
                    cashAccountCode = await NodeContext.Org_tbAccounts.Where(t => t.CashAccountName == cashAccountName).Select(t => t.CashAccountCode).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(periodName))
                {
                    FinancialPeriods periods = new(NodeContext);
                    startOn = periods.ActiveStartOn;
                    periodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                    startOn = await NodeContext.App_Periods.Where(t => t.Description == periodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                CashAccountName = cashAccountName;

                Org_CashAccount = await NodeContext.Org_CashAccounts.Where(t => t.CashAccountCode == cashAccountCode).FirstOrDefaultAsync();

                PeriodName = periodName;

                var statement = from tb in NodeContext.Cash_AccountStatements
                                where tb.CashAccountCode == cashAccountCode && tb.StartOn == startOn
                                select tb;

                Cash_AccountStatement = mode switch
                {
                    1 => await NodeContext.Cash_AccountStatements
                                    .Where(t => t.CashAccountCode == cashAccountCode)
                                    .OrderBy(t => t.EntryNumber)
                                    .ToListAsync(),
                    _ => await NodeContext.Cash_AccountStatements
                                    .Where(t => t.CashAccountCode == cashAccountCode && t.StartOn == startOn)
                                    .OrderBy(t => t.EntryNumber)
                                    .ToListAsync(),
                };
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
