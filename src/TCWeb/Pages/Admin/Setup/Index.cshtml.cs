using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.Setup
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
