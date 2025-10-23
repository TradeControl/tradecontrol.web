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
using Wangkanai.Extensions;

namespace TradeControl.Web.Pages.Cash.Statement
{
    public class IndexModel : DI_BasePageModel
    {

        // bind from query string (GET) so the select posts back work
        [BindProperty(SupportsGet = true)]
        public string AccountName { get; set; }
        public SelectList AccountNames { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public IList<Cash_vwAccountStatement> Cash_AccountStatement { get; set; }

        [BindProperty]
        public Subject_vwCashAccount Subject_CashAccount { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 50;    // default 50

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

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

                // supply a selected value so the select shows the current choice
                AccountNames = new SelectList(await accounts.ToListAsync(), AccountName ?? cashSubjectName);

                var periodNames = from tb in NodeContext.App_Periods
                                  where tb.CashStatusCode == (short)NodeEnum.CashStatus.Current || tb.CashStatusCode == (short)NodeEnum.CashStatus.Closed
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync(), periodName ?? PeriodName);

                // If the UI submitted AccountName (query string), prefer that value
                if (string.IsNullOrEmpty(cashSubjectName) && !string.IsNullOrEmpty(AccountName))
                    cashSubjectName = AccountName;

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

                // Page size options (10,50,100)
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());
                if (PageSize <= 0) PageSize = 50;

                // Base query for statements (filtered by account and optionally period)
                IQueryable<Cash_vwAccountStatement> statements = mode == 1
                    ? NodeContext.Cash_AccountStatements.Where(t => t.AccountCode == cashSubjectCode)
                    : NodeContext.Cash_AccountStatements.Where(t => t.AccountCode == cashSubjectCode && t.StartOn == startOn);

                // compute totals for pager
                TotalItems = await statements.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                try
                {
                    Cash_AccountStatement = await statements
                        .OrderByDescending(t => t.PaidOn)
                        .Skip((PageNumber - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();
                }
                catch
                {
                    Cash_AccountStatement = new List<Cash_vwAccountStatement>();
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
