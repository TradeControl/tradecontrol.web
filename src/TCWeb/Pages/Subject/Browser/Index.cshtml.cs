using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Subject.Browser
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Namespace Filter")]
        public string NamespaceFilter { get; set; }

        public async Task OnGetAsync()
        {
            await SetViewData();
        }
    }
}
