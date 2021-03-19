﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public class Periods
    {
        NodeContext _context;

        public short ActiveYear { get; }
        public DateTime ActiveStartOn { get; }
        public string ActiveYearDesc { get; }
        public string ActiveMonthName { get; }

        public Periods(NodeContext context)
        {
            try
            {
                _context = context;

                var active_period = _context.App_ActivePeriods.First();

                ActiveYear = active_period.YearNumber;
                ActiveStartOn = active_period.StartOn;
                ActiveYearDesc = active_period.Description;
                ActiveMonthName = active_period.MonthName;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
            }
        }

        public async Task<bool> Generate()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Cash.proc_GeneratePeriods");
                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> ClosePeriod()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("App.proc_PeriodClose");
                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<short> GetYearFromPeriod(DateTime startOn) => await _context.GetYearFromPeriod(startOn);

        public async Task<DateTime> AdjustToCalendar(DateTime sourceDate, short offsetDays) => await AdjustToCalendar(sourceDate, offsetDays);
    }
}
