using TradeControl.Web.Models;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public interface IInvoiceFormattingService
    {
        void Apply(Invoice_vwRegister header);
    }
}
