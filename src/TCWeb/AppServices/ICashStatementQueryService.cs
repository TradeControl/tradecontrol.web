using System;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public interface ICashStatementQueryService
    {
        Task<CashManagerStatementResult> GetStatementAsync(
            string accountCode,
            short yearNumber,
            DateTime? periodStartOn,
            string namespaceFilter,
            string cashCodeFilter,
            CancellationToken cancellationToken = default);
    }
}
