using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public interface ICashAccountMaintenanceService
    {
        Task<CashAccountEditorOptions> GetEditorOptionsAsync(CancellationToken cancellationToken = default);

        Task<CashAccountEditorModel> CreateEditorAsync(
            string? preferredAccountType,
            CancellationToken cancellationToken = default);

        Task<CashAccountEditorModel?> GetEditorAsync(
            string accountCode,
            CancellationToken cancellationToken = default);

        Task<Subject_vwCashAccount?> GetDetailsAsync(
            string accountCode,
            CancellationToken cancellationToken = default);

        Task<string> SaveAsync(
            CashAccountEditorModel model,
            string userId,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string accountCode,
            CancellationToken cancellationToken = default);
    }
}
