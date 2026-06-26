using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Register
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context)
        {
        }

        public void OnGet()
        {

        }
    }

}
