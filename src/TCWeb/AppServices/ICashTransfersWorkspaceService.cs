using System;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public interface ICashTransfersWorkspaceService
    {
        Task<CashManagerTransfersWorkspaceState> GetWorkspaceAsync(
            string sourceAccountCode,
            short yearNumber,
            DateTime? periodStartOn,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            string sourceAccountCode,
            CashManagerTransferDraftModel draft,
            string aspNetUserId,
            CancellationToken cancellationToken = default);

        Task<int> PostVisibleAsync(
            string sourceAccountCode,
            short yearNumber,
            DateTime? periodStartOn,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);
    }
}
