using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Invoice.Register.Models;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public interface IInvoiceRegisterLookupService
    {
        Task<IReadOnlyList<InvoiceRegisterYearOption>> GetYearsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<InvoiceRegisterPeriodOption>> GetPeriodsAsync(short yearNumber, CancellationToken cancellationToken = default);
        Task<InvoiceRegisterPeriodOption?> GetDefaultPeriodAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<InvoiceRegisterSelectOption>> GetCashCodeOptionsAsync(CancellationToken cancellationToken = default);
    }
}
