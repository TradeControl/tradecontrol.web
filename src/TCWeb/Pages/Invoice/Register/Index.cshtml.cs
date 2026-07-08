using System.Threading.Tasks;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Invoice.Register
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context)
        {
        }

        public async Task OnGetAsync()
        {
            await SetViewData();
        }
    }

}
