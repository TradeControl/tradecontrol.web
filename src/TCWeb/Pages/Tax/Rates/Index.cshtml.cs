using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Rates
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<YearRow> Years { get; private set; }
        public SelectList YearOptions { get; private set; }

        [BindProperty(SupportsGet = true)]
        public short? YearNumber { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                var today = DateTime.Today;

                var years = await NodeContext.App_tbYears
                    .OrderByDescending(y => y.YearNumber)
                    .Select(y => new YearRow {
                        YearNumber = y.YearNumber,
                        Description = y.Description,
                        CashStatusCode = y.CashStatusCode,
                        StartMonth = y.StartMonth
                    })
                    .ToListAsync();

                YearOptions = new SelectList(
                    items: years.Select(y => new { Value = y.YearNumber, Text = y.Description }),
                    dataValueField: "Value",
                    dataTextField: "Text",
                    selectedValue: YearNumber);

                if (YearNumber.HasValue)
                    years = years.Where(y => y.YearNumber == YearNumber.Value).ToList();

                var periodsQuery = NodeContext.App_tbYearPeriods.AsQueryable();

                if (YearNumber.HasValue)
                    periodsQuery = periodsQuery.Where(p => p.YearNumber == YearNumber.Value);

                var periods = await periodsQuery
                    .OrderByDescending(p => p.YearNumber)
                    .ThenBy(p => p.StartOn) // important: financial year is by StartOn, not MonthNumber
                    .Select(p => new PeriodRow {
                        YearNumber = p.YearNumber,
                        MonthNumber = p.MonthNumber,
                        StartOn = p.StartOn,
                        CashStatusCode = p.CashStatusCode,
                        CorporationTaxRate = p.CorporationTaxRate,
                        TaxAdjustment = p.TaxAdjustment,
                        VatAdjustment = p.VatAdjustment,
                        IsPast = p.StartOn.Date < today,
                        IsCurrent = p.StartOn.Month == today.Month && p.StartOn.Year == today.Year,
                        IsFuture = p.StartOn.Date > today
                    })
                    .ToListAsync();

                var byYear = periods
                    .GroupBy(p => p.YearNumber)
                    .ToDictionary(g => g.Key, g => (IList<PeriodRow>)g.ToList());

                foreach (var year in years)
                {
                    if (byYear.TryGetValue(year.YearNumber, out var yearPeriods))
                        year.Periods = yearPeriods;
                }

                Years = years;
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public sealed class YearRow
        {
            public short YearNumber { get; init; }
            public string Description { get; init; }
            public short CashStatusCode { get; init; }
            public short StartMonth { get; init; }
            public IList<PeriodRow> Periods { get; set; } = [];
        }

        public sealed class PeriodRow
        {
            public short YearNumber { get; init; }
            public short MonthNumber { get; init; }
            public DateTime StartOn { get; init; }
            public short CashStatusCode { get; init; }
            public float CorporationTaxRate { get; init; }
            public decimal TaxAdjustment { get; init; }
            public decimal VatAdjustment { get; init; }

            public bool IsPast { get; init; }
            public bool IsCurrent { get; init; }
            public bool IsFuture { get; init; }
        }
    }
}
