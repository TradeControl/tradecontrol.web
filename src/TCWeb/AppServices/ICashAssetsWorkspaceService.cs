using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public interface ICashAssetsWorkspaceService
    {
        Task<CashManagerAssetsWorkspaceState> GetWorkspaceAsync(
            string accountCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);

        Task<CashManagerAssetSubjectSelection> ResolveSubjectAsync(
            string namespaceFilter,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CashManagerPostedPaymentSearchResult>> SearchPostedPaymentsAsync(
            string assetAccountCode,
            string namespaceFilter,
            string searchText,
            string aspNetUserId,
            bool isPrivileged,
            int take = 40,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CashManagerPostedPaymentSearchResult>> SearchPostedCashPaymentsAsync(
            string assetAccountCode,
            string namespaceFilter,
            string searchText,
            string aspNetUserId,
            bool isPrivileged,
            int take = 40,
            CancellationToken cancellationToken = default);

        Task AddFreehandAsync(
            string accountCode,
            CashManagerAssetDraftModel draft,
            string aspNetUserId,
            CancellationToken cancellationToken = default);

        Task AddFromPaymentAsync(
            string accountCode,
            string sourcePaymentCode,
            CashManagerAssetDraftModel draft,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);

        Task PostAsync(
            string paymentCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string paymentCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);
    }
}
