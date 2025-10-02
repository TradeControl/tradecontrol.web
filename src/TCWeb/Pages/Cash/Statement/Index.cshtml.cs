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
        public string AccountName { get; set; }
        public SelectList AccountNames { get; set; }

        [BindProperty]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public IList<Cash_vwAccountStatement> Cash_AccountStatement { get; set; }

        [BindProperty]
        public Subject_vwCashAccount Subject_CashAccount { get; set; }

        public IndexModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync(string cashSubjectCode, string cashSubjectName, string periodName, string paymentCode, short mode)
        {
            try
            {
                await SetViewData();

                DateTime startOn = DateTime.Today;

                var accounts = from tb in NodeContext.Subject_tbAccounts
                               where !tb.AccountClosed
                               orderby tb.AccountTypeCode, tb.AccountName
                               select tb.AccountName;

                AccountNames = new SelectList(await accounts.ToListAsync());

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
                        cashSubjectCode = payment.AccountCode;
                        startOn = payment.StartOn;
                        periodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                        cashSubjectName = await NodeContext.Subject_tbAccounts.Where(t => t.AccountCode == cashSubjectCode).Select(t => t.AccountName).FirstOrDefaultAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(cashSubjectCode))
                {
                    cashSubjectName = await NodeContext.Subject_tbAccounts.Where(t => t.AccountCode == cashSubjectCode).Select(t => t.AccountName).FirstOrDefaultAsync();
                }
                else if (string.IsNullOrEmpty(cashSubjectName))
                {
                    CashAccounts cashAccounts = new(NodeContext);
                    cashSubjectCode = await cashAccounts.CurrentAccount();
                    cashSubjectName = await NodeContext.Subject_tbAccounts.Where(t => t.AccountCode == cashSubjectCode).Select(t => t.AccountName).FirstOrDefaultAsync();
                }
                else
                    cashSubjectCode = await NodeContext.Subject_tbAccounts.Where(t => t.AccountName == cashSubjectName).Select(t => t.AccountCode).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(periodName))
                {
                    FinancialPeriods periods = new(NodeContext);
                    startOn = periods.ActiveStartOn;
                    periodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                    startOn = await NodeContext.App_Periods.Where(t => t.Description == periodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                AccountName = cashSubjectName;

                Subject_CashAccount = await NodeContext.Subject_CashAccounts.Where(t => t.AccountCode == cashSubjectCode).FirstOrDefaultAsync();

                PeriodName = periodName;

                var statement = from tb in NodeContext.Cash_AccountStatements
                                where tb.AccountCode == cashSubjectCode && tb.StartOn == startOn
                                select tb;

                Cash_AccountStatement = mode switch
                {
                    1 => await NodeContext.Cash_AccountStatements
                                    .Where(t => t.AccountCode == cashSubjectCode)
                                    .OrderBy(t => t.EntryNumber)
                                    .ToListAsync(),
                    _ => await NodeContext.Cash_AccountStatements
                                    .Where(t => t.AccountCode == cashSubjectCode && t.StartOn == startOn)
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
