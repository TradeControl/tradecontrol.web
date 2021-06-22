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

namespace TradeControl.Web.Pages.Org.Enquiry
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public SelectList OrganisationTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string OrganisationType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IList<Org_vwAccountLookup> Org_AccountLookup { get; set; }

        public async Task OnGetAsync(string accountCode, string organisationType)
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

                var accounts = from tb in NodeContext.Org_AccountLookup select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    OrganisationType = await (from org in NodeContext.Org_tbOrgs
                                              join tp in NodeContext.Org_tbTypes on org.OrganisationTypeCode equals tp.OrganisationTypeCode
                                              where org.AccountCode == accountCode
                                              select tp.OrganisationType).FirstOrDefaultAsync();

                    accounts = accounts.Where(a => a.AccountCode == accountCode);
                }
                else
                    accounts = accounts.Where(a => a.OrganisationType == OrganisationType);

                if (!string.IsNullOrEmpty(SearchString))
                    accounts = accounts.Where(a => a.AccountName.Contains(SearchString));


                Org_AccountLookup = await accounts.OrderBy(a => a.AccountName).ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}

