using System.Threading.Tasks;
using TradeControl.Web.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TradeControl.Web.Pages.Admin.Manager
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
