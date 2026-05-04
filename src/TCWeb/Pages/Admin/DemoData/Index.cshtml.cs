using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.DemoData
{
    [Authorize(Roles = "Administrators")]
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IActionResult OnGetAsync()
        {
            return Page();
        }
    }
}
