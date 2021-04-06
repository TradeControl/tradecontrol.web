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
        protected IAuthorizationService AuthorizationService { get; }
        protected UserManager<TradeControlWebUser> UserManager { get; }

        private readonly ILogger<IndexModel> _logger = null;

        public DI_BasePageModel(
            NodeContext nodeContext,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager) : base()
        {
            NodeContext = nodeContext;
            UserManager = userManager;
            AuthorizationService = authorizationService;            
        }

        public DI_BasePageModel(
            ILogger<IndexModel> logger,
            NodeContext nodeContext,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager) : base()
        {
            _logger = logger;
            NodeContext = nodeContext;
            UserManager = userManager;
            AuthorizationService = authorizationService;
        }

        protected async Task SetViewData()
        {
            if (!ViewData.ContainsKey("CompanyName"))
                ViewData.Add("CompanyName", await NodeContext.CompanyName);
        }
    }
}
