using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices
{
    public sealed class InvoiceTypeLookup : IInvoiceTypeLookup
    {
        private NodeContext NodeContext { get; }

        public InvoiceTypeLookup(NodeContext nodeContext)
        {
            NodeContext = nodeContext;
        }

        public Task<string?> GetInvoiceTypeNameAsync(short invoiceTypeCode, CancellationToken cancellationToken = default)
        {
            return NodeContext.Invoice_tbTypes
                .Where(t => t.InvoiceTypeCode == invoiceTypeCode)
                .Select(t => t.InvoiceType)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
