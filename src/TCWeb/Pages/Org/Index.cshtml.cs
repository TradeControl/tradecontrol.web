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
using TradeControl.Web.Data;
using TradeControl.Web.Models;

using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace TradeControl.Web.Pages.Org
{
    public class IndexModel : DI_BasePageModel
    {

        const string SessionKeyReturnUrl = "_returnUrlOrgIndex";
        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl);  }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public SelectList OrganisationTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string OrganisationType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IList<Org_vwAccountLookup> Org_AccountLookup { get; set; }

        public async Task OnGetAsync(string returnUrl)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var orgTypes = from tb in NodeContext.Org_tbTypes
                               orderby tb.OrganisationType
                               select tb.OrganisationType;

                OrganisationTypes = new SelectList(await orgTypes.ToListAsync());

                var accounts = from tb in NodeContext.Org_AccountLookup
                               select tb;

                if (!string.IsNullOrEmpty(OrganisationType))
                    accounts = accounts.Where(a => a.OrganisationType == OrganisationType);

                if (!string.IsNullOrEmpty(SearchString))
                    accounts = accounts.Where(a => a.AccountName.Contains(SearchString));

                Org_AccountLookup = await accounts.OrderBy(a => a.AccountName).ToListAsync();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
