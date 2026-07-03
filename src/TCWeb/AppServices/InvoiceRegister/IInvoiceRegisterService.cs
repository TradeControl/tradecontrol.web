using System.Threading.Tasks;
using TradeControl.Web.Pages.Invoice.Register.Models;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public interface IInvoiceRegisterService
    {
        Task<InvoiceRegisterResult> QueryAsync(InvoiceFilterModel filter);
    }
}
