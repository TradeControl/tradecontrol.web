using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Invoice.Register
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context)
        {
        }

        [BindProperty(SupportsGet = true)]
        public string? InvoiceNumber { get; set; }

        public async Task OnGetAsync()
        {
            await SetViewData();
        }
    }

}
