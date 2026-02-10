using System.Threading;
using System.Threading.Tasks;

namespace TradeControl.Web.AppServices
{
    public interface IInvoiceTypeLookup
    {
        Task<string?> GetInvoiceTypeNameAsync(short invoiceTypeCode, CancellationToken cancellationToken = default);
    }
}
