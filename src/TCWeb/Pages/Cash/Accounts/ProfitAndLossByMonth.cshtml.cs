using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public class ProfitAndLossByMonthModel : DI_BasePageModel
    {
        public ProfitAndLossByMonthModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public string PeriodNamePrevious { get; set; }

        public partial class ProfitAndLosses
        {
            [StringLength(10)]
            [Display(Name = "Category Code")]
            public string CategoryCode { get; set; }
            [StringLength(50)]
            [Display(Name = "Category")]
            public string Category { get; set; }
            [DataType(DataType.Currency)]
            public decimal CurrentValue { get; set; }
            [DataType(DataType.Currency)]
            public decimal PreviousValue { get; set; }
        }

        public IList<ProfitAndLosses> Cash_ProfitAndLoss { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var periodNames = from tb in NodeContext.App_Periods
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
    }
}
