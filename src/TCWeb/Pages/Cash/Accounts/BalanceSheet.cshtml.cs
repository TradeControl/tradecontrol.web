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
    public class BalanceSheetModel : DI_BasePageModel
    {
        public BalanceSheetModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public string PeriodNamePrevious { get; set; }

        public partial class BalanceSheets
        {
            [StringLength(10)]
            [Display(Name = "Asset Code")]
            public string AssetCode { get; set; }
            [StringLength(50)]
            [Display(Name = "Asset Class")]
            public string AssetName { get; set; }
            [DataType(DataType.Currency)]
            public double CurrentBalance { get; set; }
            [DataType(DataType.Currency)]
            public double PreviousBalance { get; set; }
        }

        public IList<BalanceSheets> Cash_BalanceSheet { get; set; }

        const string CAPITAL_CODE = "CAPITAL";

        public async Task OnGetAsync()
        {
            try
            {
                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());
                Cash_BalanceSheet = new List<BalanceSheets>();

                DateTime startOn = DateTime.Today;

                if (string.IsNullOrEmpty(PeriodName))
                {
                    FinancialPeriods periods = new(NodeContext);
                    startOn = periods.ActiveStartOn;
                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                {
                    startOn = await NodeContext.App_Periods.Where(t => t.Description == PeriodName).Select(t => t.StartOn).FirstOrDefaultAsync();
                    await GenerateBalanceSheet(startOn);
                }                

                await SetViewData();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }



        private async Task GenerateBalanceSheet(DateTime startOn)
        {
            var period = await NodeContext.App_tbYearPeriods.Where(t => t.StartOn == startOn).FirstAsync();
            
            var balance_sheet = await ( from tb in NodeContext.Cash_BalanceSheet
                                        where tb.MonthNumber == period.MonthNumber && tb.StartOn <= startOn
                                        orderby tb.YearNumber descending, tb.CashModeCode descending, tb.LiquidityLevel descending, tb.EntryNumber descending
                                        select tb
                                        ).ToListAsync();

            

            foreach(var balance in balance_sheet.Where(b => b.YearNumber == period.YearNumber))
            {
                Cash_BalanceSheet.Add(new BalanceSheets()
                {
                    AssetCode = balance.AssetCode,
                    AssetName = balance.AssetName,
                    CurrentBalance = balance.Balance
                });
            }

            if (balance_sheet.Where(b => b.YearNumber < period.YearNumber).Any())
            {
                short yearNumber = balance_sheet.Where(b => b.YearNumber < period.YearNumber).Max(b => b.YearNumber);

                PeriodNamePrevious = await NodeContext.App_Periods
                                                        .Where(t => t.YearNumber == yearNumber && t.MonthNumber == period.MonthNumber)
                                                        .Select(t => t.Description).FirstOrDefaultAsync();

                foreach (var balance in balance_sheet.Where(b => b.YearNumber == yearNumber))
                {
                    var asset = Cash_BalanceSheet.Where(a => a.AssetCode == balance.AssetCode).FirstOrDefault();
                    if (asset != null)
                        asset.PreviousBalance = balance.Balance;
                    else
                        Cash_BalanceSheet.Add(new BalanceSheets()
                        {
                            AssetCode = balance.AssetCode,
                            AssetName = balance.AssetName,
                            CurrentBalance = 0,
                            PreviousBalance = balance.Balance
                        });
                }
            }

            Cash_BalanceSheet.Add(new BalanceSheets()
            {
                AssetCode = CAPITAL_CODE,
                AssetName = CAPITAL_CODE,
                CurrentBalance = Cash_BalanceSheet.Sum(b => b.CurrentBalance),
                PreviousBalance = Cash_BalanceSheet.Sum(b => b.PreviousBalance)
            });

        }
    }
}
