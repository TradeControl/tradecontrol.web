using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Invoice.Register.Models;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public sealed class InvoiceRegisterLookupService : IInvoiceRegisterLookupService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InvoiceRegisterLookupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<IReadOnlyList<InvoiceRegisterYearOption>> GetYearsAsync(CancellationToken cancellationToken = default)
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
                .Select(year => new InvoiceRegisterYearOption(
                    year.YearNumber,
                    year.Description))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<InvoiceRegisterPeriodOption>> GetPeriodsAsync(
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
                .Select(period => new InvoiceRegisterPeriodOption(
                    period.YearNumber,
                    period.MonthNumber,
                    period.StartOn,
                    period.Description,
                    (NodeEnum.CashStatus)period.CashStatusCode))
                .ToListAsync(cancellationToken);
        }

        public async Task<InvoiceRegisterPeriodOption?> GetDefaultPeriodAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var currentPeriod = await nodeContext.App_Periods
                .AsNoTracking()
                .Where(period => period.CashStatusCode == (short)NodeEnum.CashStatus.Current)
                .OrderByDescending(period => period.StartOn)
                .Select(period => new InvoiceRegisterPeriodOption(
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
                .Select(period => new InvoiceRegisterPeriodOption(
                    period.YearNumber,
                    period.MonthNumber,
                    period.StartOn,
                    period.Description,
                    (NodeEnum.CashStatus)period.CashStatusCode))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<InvoiceRegisterSelectOption>> GetCashCodeOptionsAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            return await nodeContext.Cash_CodeLookup
                .AsNoTracking()
                .OrderBy(code => code.CashDescription)
                .Select(code => new InvoiceRegisterSelectOption(
                    code.CashCode,
                    code.CashDescription))
                .ToListAsync(cancellationToken);
        }
    }
}
