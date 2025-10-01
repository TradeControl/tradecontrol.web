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

namespace TradeControl.Web.Pages.Subject.Enquiry
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public SelectList SubjectTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SubjectType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IList<Subject_vwAccountLookup> Subject_AccountLookup { get; set; }

        public async Task OnGetAsync(string accountCode, string subjectType)
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
                else if (string.IsNullOrEmpty(SubjectType))
                    SubjectType = orgTypes.First();

                var accounts = from tb in NodeContext.Subject_AccountLookup select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    SubjectType = await (from subject in NodeContext.Subject_tbSubjects
                                              join tp in NodeContext.Subject_tbTypes on subject.SubjectTypeCode equals tp.SubjectTypeCode
                                              where subject.AccountCode == accountCode
                                              select tp.SubjectType).FirstOrDefaultAsync();

                    accounts = accounts.Where(a => a.AccountCode == accountCode);
                }
                else
                    accounts = accounts.Where(a => a.SubjectType == SubjectType);

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

