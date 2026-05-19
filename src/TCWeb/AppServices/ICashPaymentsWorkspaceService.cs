using System;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public interface ICashPaymentsWorkspaceService
    {
        Task<CashManagerPaymentsWorkspaceState> GetWorkspaceAsync(
            string accountCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);

        Task<CashManagerOrganisationCreationResult> CreateOrganisationAsync(
            CashManagerOrganisationDraftModel model,
            CancellationToken cancellationToken = default);

        Task<CashManagerOrganisationCreationResult> ResolveOrganisationAsync(
            string namespaceFilter,
            CancellationToken cancellationToken = default);

        Task AddPaymentAsync(
            string accountCode,
            CashManagerPaymentLineModel draft,
            string aspNetUserId,
            CancellationToken cancellationToken = default);

        Task UpdatePaymentAsync(
            CashManagerPaymentLineModel payment,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);

        Task DeletePaymentAsync(
            string paymentCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default);

        Task PostAsync(
            string aspNetUserId,
            CancellationToken cancellationToken = default);

        Task<NodeEnum.CashStatus> GetPeriodStatusAsync(
            DateTime paidOn,
            CancellationToken cancellationToken = default);

        Task<string> GetDefaultTaxCodeForCashCodeAsync(
            string cashCode,
            CancellationToken cancellationToken = default);
    }
}
