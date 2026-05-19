using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public sealed class CashManagerService : ICashManagerService
    {
        private readonly NodeContext _nodeContext;

        public CashManagerService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<IReadOnlyList<CashManagerAccountSummary>> GetAccountsAsync(
            bool includeClosed = false,
            CancellationToken cancellationToken = default)
        {
            var query = _nodeContext.Subject_CashAccounts
                .AsNoTracking();

            if (!includeClosed)
            {
                query = query.Where(account => !account.AccountClosed);
            }

            return await query
                .OrderBy(account => account.AccountTypeCode)
                .ThenBy(account => account.AccountClosed)
                .ThenBy(account => account.LiquidityLevel)
                .ThenBy(account => account.AccountName)
                .Select(account => new CashManagerAccountSummary(
                    account.AccountCode,
                    account.AccountName,
                    account.SubjectCode,
                    account.SubjectName,
                    (NodeEnum.CashAccountType)account.AccountTypeCode,
                    account.CurrentBalance,
                    account.LiquidityLevel,
                    account.AccountClosed))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CashManagerYearOption>> GetYearsAsync(CancellationToken cancellationToken = default)
        {
            var activeYearNumbers = await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(period => period.CashStatusCode != (short)NodeEnum.CashStatus.Archived)
                .Select(period => period.YearNumber)
                .Distinct()
                .ToListAsync(cancellationToken);

            return await _nodeContext.App_tbYears
                .AsNoTracking()
                .Where(year => activeYearNumbers.Contains(year.YearNumber))
                .OrderByDescending(year => year.YearNumber)
                .Select(year => new CashManagerYearOption(
                    year.YearNumber,
                    year.Description))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CashManagerPeriodOption>> GetPeriodsAsync(
            short yearNumber,
            CancellationToken cancellationToken = default)
        {
            return await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(period =>
                    period.YearNumber == yearNumber
                    && period.CashStatusCode != (short)NodeEnum.CashStatus.Archived)
                .OrderByDescending(period => period.StartOn)
                .Select(period => new CashManagerPeriodOption(
                    period.YearNumber,
                    period.MonthNumber,
                    period.StartOn,
                    period.Description,
                    (NodeEnum.CashStatus)period.CashStatusCode))
                .ToListAsync(cancellationToken);
        }

        public async Task<CashManagerPeriodOption?> GetDefaultPeriodAsync(CancellationToken cancellationToken = default)
        {
            var currentPeriod = await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(period => period.CashStatusCode == (short)NodeEnum.CashStatus.Current)
                .OrderByDescending(period => period.StartOn)
                .Select(period => new CashManagerPeriodOption(
                    period.YearNumber,
                    period.MonthNumber,
                    period.StartOn,
                    period.Description,
                    (NodeEnum.CashStatus)period.CashStatusCode))
                .FirstOrDefaultAsync(cancellationToken);

            if (currentPeriod is not null)
            {
                return currentPeriod;
            }

            return await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(period => period.CashStatusCode != (short)NodeEnum.CashStatus.Archived)
                .OrderByDescending(period => period.StartOn)
                .Select(period => new CashManagerPeriodOption(
                    period.YearNumber,
                    period.MonthNumber,
                    period.StartOn,
                    period.Description,
                    (NodeEnum.CashStatus)period.CashStatusCode))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
