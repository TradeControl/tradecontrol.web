using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Invoice.Register.Models;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public interface IInvoiceRegisterQueryBuilder
    {
        IQueryable<Invoice_vwRegister> BuildHeaderQuery(NodeContext nodeContext, InvoiceFilterModel filter);
        IQueryable<Invoice_vwRegisterDetail> BuildDetailQuery(NodeContext nodeContext, InvoiceFilterModel filter);
        IQueryable<Invoice_vwRegisterCashCode> BuildCashCodeQuery(NodeContext nodeContext, InvoiceFilterModel filter);
        IQueryable<Invoice_vwRegister> ApplyHeaderSorting(IQueryable<Invoice_vwRegister> query, InvoiceFilterModel filter);
        Task<List<Invoice_vwRegister>> ApplyFormattingAsync(IEnumerable<Invoice_vwRegister> headers, IInvoiceFormattingService formattingService);
    }
}
