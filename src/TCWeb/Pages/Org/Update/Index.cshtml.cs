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

namespace TradeControl.Web.Pages.Org.Update
{
    public class IndexModel : DI_BasePageModel
    {

        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public SelectList OrganisationTypes { get; set; }
        [BindProperty(SupportsGet = true)]
        public string OrganisationType { get; set; }

        public SelectList OrganisationStatuses { get; set; }
        [BindProperty]
        public string OrganisationStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IList<Org_vwAccountLookupAll> Org_AccountLookup { get; set; }

        public async Task OnGetAsync(string accountCode, string organisationStatus, string organisationType)
        {
            try
            {
                await SetViewData();

                var orgTypes = from tb in NodeContext.Org_tbTypes
                               orderby tb.OrganisationType
                               select tb.OrganisationType;

                OrganisationTypes = new SelectList(await orgTypes.ToListAsync());

                if (!string.IsNullOrEmpty(organisationType))
                    OrganisationType = organisationType;
                else if (string.IsNullOrEmpty(OrganisationType))
                    OrganisationType = orgTypes.First();

                if (!string.IsNullOrEmpty(organisationStatus))
                    OrganisationStatus = organisationStatus;
                else if (string.IsNullOrEmpty(OrganisationStatus))
                    OrganisationStatus = await NodeContext.Org_tbStatuses.Where(t => t.OrganisationStatusCode == (short)NodeEnum.OrgStatus.Active).Select(t => t.OrganisationStatus).FirstAsync();

                var orgStatus = from tb in NodeContext.Org_tbStatuses
                                orderby tb.OrganisationStatusCode
                                select tb.OrganisationStatus;

                OrganisationStatuses = new SelectList(await orgStatus.ToListAsync());

                var accounts = from tb in NodeContext.Org_AccountLookupAll select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    accounts = accounts.Where(a => a.AccountCode == accountCode);
                    var org = await accounts.FirstOrDefaultAsync();
                    OrganisationType = org.OrganisationType;
                    OrganisationStatus = org.OrganisationStatus;
                }
                else
                {
                    accounts = accounts.Where(a => a.OrganisationType == OrganisationType);
                    accounts = accounts.Where(a => a.OrganisationStatus == OrganisationStatus);
                }

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
