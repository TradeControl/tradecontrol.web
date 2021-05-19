using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Transfer
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) {}

        public IList<Cash_vwTransfersUnposted> Cash_TransfersUnposted { get; set; }

        public async Task OnGetAsync()
        {
            var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

            if (!isAuthorized)
            {
                var profile = new Profile(NodeContext);
                var user = await UserManager.GetUserAsync(User);
                string userId = await profile.UserId(user.Id);

                Cash_TransfersUnposted = await NodeContext.Cash_TransfersUnposted.Where(t => t.UserId == userId).Select(t => t).ToListAsync();
            }
            else
                Cash_TransfersUnposted = await NodeContext.Cash_TransfersUnposted.Select(t => t).ToListAsync();

            await SetViewData();
        }
    }
}
