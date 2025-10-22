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

        public IList<Subject_vwSubjectLookup> Subject_SubjectLookup { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync(string accountCode, string subjectType)
        {
            try
            {
                await SetViewData();

                var orgTypes = from tb in NodeContext.Subject_tbTypes
                               orderby tb.SubjectType
                               select tb.SubjectType;

                // materialize types, keep a default real type, and insert an "All" option at the front
                var orgTypesList = await orgTypes.ToListAsync();
                var defaultType = orgTypesList.FirstOrDefault();
                var typesWithAll = new List<string> { "All" };
                typesWithAll.AddRange(orgTypesList);

                SubjectTypes = new SelectList(typesWithAll);

                // Page size options
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());

                // respect explicit query param, otherwise preserve bound SubjectType or fall back to a sensible default
                if (!string.IsNullOrEmpty(subjectType))
                    SubjectType = subjectType;
                else if (string.IsNullOrEmpty(SubjectType))
                    SubjectType = defaultType ?? "All";

                var accounts = from tb in NodeContext.Subject_SubjectLookup select tb;

                if (!string.IsNullOrEmpty(accountCode))
                {
                    SubjectType = await (from subject in NodeContext.Subject_tbSubjects
                                         join tp in NodeContext.Subject_tbTypes on subject.SubjectTypeCode equals tp.SubjectTypeCode
                                         where subject.SubjectCode == accountCode
                                         select tp.SubjectType).FirstOrDefaultAsync();

                    accounts = accounts.Where(a => a.SubjectCode == accountCode);
                }
                else
                {
                    // If "All" is selected, do not filter by SubjectType
                    if (!string.IsNullOrEmpty(SubjectType) && SubjectType != "All")
                        accounts = accounts.Where(a => a.SubjectType == SubjectType);
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

