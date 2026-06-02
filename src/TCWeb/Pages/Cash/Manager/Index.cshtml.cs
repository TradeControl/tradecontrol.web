using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.Manager
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext nodeContext) : base(nodeContext)
        {
        }

        [BindProperty(SupportsGet = true)]
        public string? AccountCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentCode { get; set; }

        public async Task OnGetAsync()
        {
            await SetViewData();
        }
    }
}
