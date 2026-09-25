using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.Manager
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext nodeContext) : base(nodeContext)
        {
        }

        public bool IsInitialSetup { get; private set; }

        public async Task OnGetAsync()
        {
            await SetViewData();

            IsInitialSetup = !await NodeContext.App_tbOptions.AnyAsync()
                || !await NodeContext.Usr_Doc.AnyAsync();
        }
    }
}
