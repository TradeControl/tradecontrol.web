using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public interface ICashStatementPaymentMaintenanceService
    {
        Task<CashManagerStatementPaymentEditorState?> GetEditorAsync(
            string paymentCode,
            CancellationToken cancellationToken = default);

        Task SaveEditAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default);

        Task MoveAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default);

        Task SavePaymentAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default);
    }
}
