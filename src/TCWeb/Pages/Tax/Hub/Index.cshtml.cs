using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Tax.Hub
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext nodeContext) : base(nodeContext)
        {
        }

        public async Task OnGetAsync()
        {
            await SetViewData();
        }
    }
}
