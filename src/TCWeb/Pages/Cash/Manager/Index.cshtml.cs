using System.Threading.Tasks;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.Manager
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
