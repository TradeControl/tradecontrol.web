using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TradeControl.Web.Areas.Identity.Data;
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

/*



*/
        protected async Task SetViewData()
        {
            if (!ViewData.ContainsKey("CompanyName"))
                ViewData.Add("CompanyName", await NodeContext.CompanyName());
        }
    }
}
