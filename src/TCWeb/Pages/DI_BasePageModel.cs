using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages
{
    public class DI_BasePageModel : PageModel
    {
        protected NodeContext NodeContext { get; }

        public DI_BasePageModel(NodeContext nodeContext) : base()
        {
            NodeContext = nodeContext;
        }

        protected async Task SetViewData()
        {
            if (!ViewData.ContainsKey("CompanyName"))
                ViewData.Add("CompanyName", await NodeContext.CompanyName());
        }
    }
}
