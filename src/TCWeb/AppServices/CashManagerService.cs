using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public sealed class CashManagerService : ICashManagerService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CashManagerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<IReadOnlyList<CashManagerAccountSummary>> GetAccountsAsync(
            bool includeClosed = false,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var query = nodeContext.Subject_CashAccounts
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
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var activeYearNumbers = await nodeContext.App_Periods
                .AsNoTracking()
                .Where(period => period.CashStatusCode != (short)NodeEnum.CashStatus.Archived)
                .Select(period => period.YearNumber)
                .Distinct()
                .ToListAsync(cancellationToken);

            return await nodeContext.App_tbYears
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
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var query = nodeContext.App_Periods
                .AsNoTracking()
                .Where(period => period.CashStatusCode != (short)NodeEnum.CashStatus.Archived);

            if (yearNumber > 0)
            {
                query = query.Where(period => period.YearNumber == yearNumber);
            }

            return await query
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
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var currentPeriod = await nodeContext.App_Periods
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

            return await nodeContext.App_Periods
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

        public async Task<IReadOnlyList<CashManagerSelectOption>> GetCashCodeOptionsAsync(
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            return await nodeContext.Cash_tbCodes
                .AsNoTracking()
                .Where(code => code.IsEnabled != 0)
                .OrderBy(code => code.CashDescription)
                .Select(code => new CashManagerSelectOption(
                    code.CashCode,
                    code.CashDescription))
                .ToListAsync(cancellationToken);
        }

        public async Task<string?> GetAccountCodeByPaymentCodeAsync(
            string paymentCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(paymentCode))
                return null;

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            return await nodeContext.Cash_tbPayments
                .AsNoTracking()
                .Where(payment => payment.PaymentCode == paymentCode)
                .Select(payment => payment.AccountCode)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
