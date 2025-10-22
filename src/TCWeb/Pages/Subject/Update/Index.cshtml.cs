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

        public IList<Subject_vwSubjectLookupAll> Subject_SubjectLookup { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync(string accountCode, string subjectStatus, string subjectType)
        {
            try
            {
                await SetViewData();

                var orgTypes = from tb in NodeContext.Subject_tbTypes
                               orderby tb.SubjectType
                               select tb.SubjectType;

                // materialize and add "All" option
                var orgTypesList = await orgTypes.ToListAsync();
                var defaultType = orgTypesList.FirstOrDefault();
                var typesWithAll = new List<string> { "All" };
                typesWithAll.AddRange(orgTypesList);

                SubjectTypes = new SelectList(typesWithAll);

                // Page size options
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());

                // respect explicit query param, otherwise preserve bound SubjectType or fall back to sensible default
                if (!string.IsNullOrEmpty(subjectType))
                    SubjectType = subjectType;
                else if (string.IsNullOrEmpty(SubjectType))
                    SubjectType = defaultType ?? "All";

                if (!string.IsNullOrEmpty(subjectStatus))
                    SubjectStatus = subjectStatus;
                else if (string.IsNullOrEmpty(SubjectStatus))
                    SubjectStatus = await NodeContext.Subject_tbStatuses.Where(t => t.SubjectStatusCode == (short)NodeEnum.SubjectStatus.Active).Select(t => t.SubjectStatus).FirstAsync();

                var subjectStatusQuery = from tb in NodeContext.Subject_tbStatuses
                                         orderby tb.SubjectStatusCode
                                         select tb.SubjectStatus;

                SubjectStatuses = new SelectList(await subjectStatusQuery.ToListAsync());

                var accounts = from tb in NodeContext.Subject_SubjectLookupAll select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    accounts = accounts.Where(a => a.SubjectCode == accountCode);
                    var subject = await accounts.SingleOrDefaultAsync();
                    if (subject != null)
                    {
                        SubjectType = subject.SubjectType;
                        subjectType = subject.SubjectType;
                        SubjectStatus = subject.SubjectStatus;
                    }
                }
                else
                {
                    // If "All" is selected, do not filter by SubjectType
                    if (!string.IsNullOrEmpty(SubjectType) && SubjectType != "All")
                        accounts = accounts.Where(a => a.SubjectType == SubjectType);

                    if (!string.IsNullOrEmpty(subjectStatus))
                        accounts = accounts.Where(a => a.SubjectStatus == subjectStatus);
                    else if (!string.IsNullOrEmpty(SubjectStatus))
                        accounts = accounts.Where(a => a.SubjectStatus == SubjectStatus);
                }

                if (!string.IsNullOrEmpty(SearchString))
                    accounts = accounts.Where(a => a.SubjectName.Contains(SearchString));

                // compute totals for pager
                TotalItems = await accounts.CountAsync();

                // protect PageSize
                if (PageSize <= 0) PageSize = 10;

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                Subject_SubjectLookup = await accounts
                    .OrderBy(a => a.SubjectName)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
