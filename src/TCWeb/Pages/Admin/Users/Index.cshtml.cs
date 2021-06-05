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

using TradeControl.Web.Authorization;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Users
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public SelectList ConfirmFilterOptions { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ConfirmFilterOption { get; set; }

        public IList<AspNet_UserRegistration> AspNet_UserRegistration { get;set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                ConfirmFilterOptions = new SelectList(new List<string>() { "No", "Yes" });

                if (!string.IsNullOrEmpty(ConfirmFilterOption))
                {
                    bool IsConfirmed = ConfirmFilterOption == "Yes";
                    AspNet_UserRegistration = await NodeContext.AspNet_UserRegistrations.Where(u => u.IsConfirmed == IsConfirmed).ToListAsync();
                }
                else
                    AspNet_UserRegistration = await NodeContext.AspNet_UserRegistrations.ToListAsync();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }

        }
    }
}
