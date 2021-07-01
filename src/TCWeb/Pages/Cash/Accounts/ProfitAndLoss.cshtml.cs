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
    public partial class ProfitAndLosses
    {
        [StringLength(10)]
        [Display(Name = "Code")]
        public string CategoryCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; }
        [DataType(DataType.Currency)]
        public decimal CurrentValue { get; set; }
        [DataType(DataType.Currency)]
        public decimal PreviousValue { get; set; }
    }

    public class ProfitAndLossModel : DI_BasePageModel
    {
        public ProfitAndLossModel(NodeContext context) : base(context) { }

        [BindProperty]
        public string DetailsHtml { get; set; }

        [BindProperty(SupportsGet = true)]
        public string YearName { get; set; }
        public SelectList YearNames { get; set; }

        [BindProperty]
        public string YearNamePrevious { get; set; }

        public IList<ProfitAndLosses> Cash_ProfitAndLoss { get; set; }

        public async Task OnGetAsync(int? mode)
        {
            try
            {
                var yearNames = from tb in NodeContext.App_tbYears
                                where tb.CashStatusCode > 0 && tb.CashStatusCode < 3
                                orderby tb.YearNumber descending
                                select tb.Description;

                YearNames = new SelectList(await yearNames.ToListAsync());
                Cash_ProfitAndLoss = new List<ProfitAndLosses>();

                short yearNumber;

                if (string.IsNullOrEmpty(YearName))
                {
                    FinancialPeriods periods = new(NodeContext);
                    yearNumber = periods.ActiveYear;
                    YearName = await NodeContext.App_tbYears.Where(t => t.YearNumber == yearNumber).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                    yearNumber = await NodeContext.App_tbYears.Where(t => t.Description == YearName).Select(t => t.YearNumber).FirstOrDefaultAsync();

                await GenerateProfitAndLoss(yearNumber);

                if (mode != null)
                    await GenerateProfitAndLossDetails(yearNumber);

                await SetViewData();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task GenerateProfitAndLoss(short yearNumber)
        {
            var profit_and_loss = await (from tb in NodeContext.Cash_ProfitAndLossByYear
                                       orderby tb.YearNumber descending, tb.DisplayOrder
                                       select tb
                                        ).ToListAsync();

            foreach (var invoice_value in profit_and_loss.Where(b => b.YearNumber == yearNumber))
            {
                Cash_ProfitAndLoss.Add(new ProfitAndLosses()
                {
                    CategoryCode = invoice_value.CategoryCode,
                    Category = invoice_value.Category,
                    CurrentValue = invoice_value.InvoiceValue
                });
            }

            if (profit_and_loss.Where(b => b.YearNumber < yearNumber).Any())
            {
                yearNumber = profit_and_loss.Where(b => b.YearNumber < yearNumber).Max(b => b.YearNumber);

                YearNamePrevious = await NodeContext.App_tbYears
                                                        .Where(t => t.YearNumber == yearNumber)
                                                        .Select(t => t.Description).FirstOrDefaultAsync();

                foreach (var invoice_value in profit_and_loss.Where(b => b.YearNumber == yearNumber))
                {
                    var category = Cash_ProfitAndLoss.Where(a => a.CategoryCode == invoice_value.CategoryCode).First();
                    category.PreviousValue = invoice_value.InvoiceValue;
                }
            }
        }

        private async Task GenerateProfitAndLossDetails(short yearNumber)
        {
            StringBuilder details = new();
            details.AppendLine(@"<h2>Details</h2>");
            details.AppendLine(@"<table class=""table table-striped"">");
            details.AppendLine(@"<thead><tr>");
            details.AppendLine($"<th>{GetDisplayName<ProfitAndLosses>("CategoryCode")}</th>");
            details.AppendLine($"<th>{GetDisplayName<ProfitAndLosses>("Category")}</th>");
            details.AppendLine($"<th>{YearName}</th>");
            details.AppendLine($"<th>{YearNamePrevious}</th>");
            details.AppendLine("</tr></thead>");
            details.AppendLine("<tbody>");

            var categories = await NodeContext.Cash_FlowCategories.OrderBy(c => c.EntryId).ToListAsync();

            foreach (var category in categories)
            {
                details.AppendLine(string.Concat(@"<tr><td colspan=""4""><strong>", category.Category, "</strong></td></tr>"));

                var cash_codes = await ( from tb in NodeContext.Cash_tbCodes
                                    where tb.CategoryCode == category.CategoryCode && tb.IsEnabled != 0
                                    select tb).ToListAsync();

                decimal currentYearTotal = 0, lastYearTotal = 0;

                foreach (var cash_code in cash_codes)
                {                                        
                    decimal? currentYear = await NodeContext.Cash_FlowCategoryByYears.Where(c => c.YearNumber == yearNumber && c.CashCode == cash_code.CashCode).Select(s => s.InvoiceValue).SingleOrDefaultAsync();
                    decimal? lastYear = await NodeContext.Cash_FlowCategoryByYears.Where(c => c.YearNumber == yearNumber - 1 && c.CashCode == cash_code.CashCode).Select(s => s.InvoiceValue).SingleOrDefaultAsync();
                    
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
