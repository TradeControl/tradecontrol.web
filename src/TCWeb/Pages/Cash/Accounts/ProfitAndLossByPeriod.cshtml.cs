using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Accounts
{
    public class ProfitAndLossByPeriodModel : DI_BasePageModel
    {
        [BindProperty]
        public string DetailsHtml { get; set; }


        [BindProperty(SupportsGet = true)]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public string PeriodNamePrevious { get; set; }

        public IList<ProfitAndLosses> Cash_ProfitAndLoss { get; set; }

        public ProfitAndLossByPeriodModel(NodeContext context) : base(context) { }


        public async Task OnGetAsync(int? mode)
        {
            try
            {
                var periodNames = from tb in NodeContext.App_Periods
                                  where tb.CashStatusCode < (short)NodeEnum.CashStatus.Archived && tb.CashStatusCode > (short)NodeEnum.CashStatus.Forecast
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());
                Cash_ProfitAndLoss = new List<ProfitAndLosses>();

                DateTime startOn = DateTime.Today;

                if (string.IsNullOrEmpty(PeriodName))
                {
                    FinancialPeriods periods = new(NodeContext);
                    startOn = periods.ActiveStartOn;
                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                    startOn = await NodeContext.App_Periods.Where(t => t.Description == PeriodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                await GenerateProfitAndLoss(startOn);

                if (mode != null)
                    await GenerateProfitAndLossDetails(startOn);

                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task GenerateProfitAndLoss(DateTime startOn)
        {
            var period = await NodeContext.App_tbYearPeriods.Where(t => t.StartOn == startOn).FirstAsync();

            var profit_and_loss = await (from tb in NodeContext.Cash_ProfitAndLossByMonth
                                       where tb.MonthNumber == period.MonthNumber && tb.StartOn <= startOn
                                       orderby tb.YearNumber descending, tb.DisplayOrder
                                       select tb
                                        ).ToListAsync();


            foreach (var invoice_value in profit_and_loss.Where(b => b.YearNumber == period.YearNumber))
            {
                Cash_ProfitAndLoss.Add(new ProfitAndLosses()
                {
                    CategoryCode = invoice_value.CategoryCode,
                    Category = invoice_value.Category,
                    CurrentValue = invoice_value.InvoiceValue
                });
            }

            if (profit_and_loss.Where(b => b.YearNumber < period.YearNumber).Any())
            {
                short yearNumber = profit_and_loss.Where(b => b.YearNumber < period.YearNumber).Max(b => b.YearNumber);

                PeriodNamePrevious = await NodeContext.App_Periods
                                                        .Where(t => t.YearNumber == yearNumber && t.MonthNumber == period.MonthNumber)
                                                        .Select(t => t.Description).FirstOrDefaultAsync();

                foreach (var invoice_value in profit_and_loss.Where(b => b.YearNumber == yearNumber))
                {
                    var category = Cash_ProfitAndLoss.Where(a => a.CategoryCode == invoice_value.CategoryCode).FirstOrDefault();
                    if (category != null)
                        category.PreviousValue = invoice_value.InvoiceValue;
                    else
                        Cash_ProfitAndLoss.Add(new ProfitAndLosses()
                        {
                            CategoryCode = invoice_value.CategoryCode,
                            Category = invoice_value.Category,
                            CurrentValue = 0,
                            PreviousValue = invoice_value.InvoiceValue
                        });
                }
            }
        }

        private async Task GenerateProfitAndLossDetails(DateTime startOn)
        {
            StringBuilder details = new();
            details.AppendLine(@"<h2>Details</h2>");
            details.AppendLine(@"<table class=""table table-striped"">");
            details.AppendLine(@"<thead><tr>");
            details.AppendLine($"<th>{GetDisplayName<ProfitAndLosses>("CategoryCode")}</th>");
            details.AppendLine($"<th>{GetDisplayName<ProfitAndLosses>("Category")}</th>");
            details.AppendLine($"<th>{PeriodName}</th>");
            details.AppendLine($"<th>{PeriodNamePrevious}</th>");
            details.AppendLine("</tr></thead>");
            details.AppendLine("<tbody>");

            var period = await NodeContext.App_tbYearPeriods.Where(p => p.StartOn == startOn).SingleAsync();
            DateTime? previousStartOn = null;

            if (await NodeContext.App_tbYearPeriods.Where(p => p.YearNumber == period.YearNumber - 1 && p.MonthNumber == period.MonthNumber).AnyAsync())
                previousStartOn = await NodeContext.App_tbYearPeriods
                                        .Where(p => p.YearNumber == period.YearNumber - 1 && p.MonthNumber == period.MonthNumber)
                                        .Select(p => p.StartOn)
                                        .SingleAsync();

            var categories = await NodeContext.Cash_FlowCategories.OrderBy(c => c.EntryId).ToListAsync();

            foreach (var category in categories)
            {
                details.AppendLine(string.Concat(@"<tr><td colspan=""4""><strong>", category.Category, "</strong></td></tr>"));

                var cash_codes = await (from tb in NodeContext.Cash_tbCodes
                                        where tb.CategoryCode == category.CategoryCode && tb.IsEnabled != 0
                                        select tb).ToListAsync();

                decimal currentYearTotal = 0, lastYearTotal = 0;

                foreach (var cash_code in cash_codes)
                {
                    decimal? currentYear = await NodeContext.Cash_FlowCategoryByPeriods.Where(c => c.StartOn == startOn && c.CashCode == cash_code.CashCode).Select(s => s.InvoiceValue).SingleOrDefaultAsync();
                    decimal? lastYear = null; 
                    
                    if (previousStartOn != null)
                        lastYear = await NodeContext.Cash_FlowCategoryByPeriods.Where(c => c.StartOn == previousStartOn && c.CashCode == cash_code.CashCode).Select(s => s.InvoiceValue).SingleOrDefaultAsync();

                    currentYear = currentYear == null ? 0 : currentYear;
                    lastYear = lastYear == null ? 0 : lastYear;
                    currentYearTotal += (decimal)currentYear;
                    lastYearTotal += (decimal)lastYear;

                    details.AppendLine($"<tr><td>{cash_code.CashCode}</td><td>{cash_code.CashDescription}</td>");
                    details.AppendLine($"<td>{currentYear:C2}</td><td>{lastYear:C2}</td>");
                    details.AppendLine("</tr>");
                }

                details.AppendLine($"<tr><td><strong>{category.CategoryCode}</strong></td><td></td>");
                details.AppendLine($"<td><strong>{currentYearTotal:C2}</strong></td><td><strong>{lastYearTotal:C2}</strong></td>");
                details.AppendLine("</tr>");

            }

            details.AppendLine("</tbody>");
            details.AppendLine("</table>");

            DetailsHtml = details.ToString();
        }

        protected string GetDisplayName<T>(string propertyName)
        {
            MemberInfo property = typeof(T).GetProperty(propertyName);
            var attribute = property.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();
            return attribute?.Name;
        }


    }
}
