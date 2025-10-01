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

namespace TradeControl.Web.Pages.Subject.Update
{
    public class IndexModel : DI_BasePageModel
    {

        public IndexModel(NodeContext context) : base(context) { }

        public SelectList SubjectTypes { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SubjectType { get; set; }

        public SelectList SubjectStatuses { get; set; }

        [BindProperty]
        public string SubjectStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IList<Subject_vwAccountLookupAll> Subject_AccountLookup { get; set; }

        public async Task OnGetAsync(string accountCode, string subjectStatus, string subjectType)
        {
            try
            {
                await SetViewData();

                var orgTypes = from tb in NodeContext.Subject_tbTypes
                               orderby tb.SubjectType
                               select tb.SubjectType;

                SubjectTypes = new SelectList(await orgTypes.ToListAsync());

                if (!string.IsNullOrEmpty(subjectType))
                    SubjectType = subjectType;
                else if (string.IsNullOrEmpty(subjectType))
                    SubjectType = orgTypes.First();

                if (!string.IsNullOrEmpty(subjectStatus))
                    SubjectStatus = subjectStatus;
                else if (string.IsNullOrEmpty(SubjectStatus))
                    SubjectStatus = await NodeContext.Subject_tbStatuses.Where(t => t.SubjectStatusCode == (short)NodeEnum.SubjectStatus.Active).Select(t => t.SubjectStatus).FirstAsync();

                var subjectStatusQuery = from tb in NodeContext.Subject_tbStatuses
                                         orderby tb.SubjectStatusCode
                                         select tb.SubjectStatus;

                SubjectStatuses = new SelectList(await subjectStatusQuery.ToListAsync());

                var accounts = from tb in NodeContext.Subject_AccountLookupAll select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    accounts = accounts.Where(a => a.AccountCode == accountCode);
                    var subject = await accounts.SingleOrDefaultAsync();
                    subjectType = subject.SubjectType;
                    SubjectStatus = subject.SubjectStatus;
                }
                else
                {
                    accounts = accounts.Where(a => a.SubjectType == subjectType);
                    accounts = accounts.Where(a => a.SubjectStatus == subjectStatus);
                }

                if (!string.IsNullOrEmpty(SearchString))
                    accounts = accounts.Where(a => a.AccountName.Contains(SearchString));


                Subject_AccountLookup = await accounts.OrderBy(a => a.AccountName).ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
