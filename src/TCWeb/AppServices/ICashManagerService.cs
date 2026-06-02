using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public interface ICashManagerService
    {
        Task<IReadOnlyList<CashManagerAccountSummary>> GetAccountsAsync(
            bool includeClosed = false,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CashManagerYearOption>> GetYearsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CashManagerPeriodOption>> GetPeriodsAsync(short yearNumber, CancellationToken cancellationToken = default);
        Task<CashManagerPeriodOption?> GetDefaultPeriodAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CashManagerSelectOption>> GetCashCodeOptionsAsync(
            CancellationToken cancellationToken = default);

        Task<string?> GetAccountCodeByPaymentCodeAsync(
            string paymentCode,
            CancellationToken cancellationToken = default);
    }
}
