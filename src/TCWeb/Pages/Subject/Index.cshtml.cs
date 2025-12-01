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

namespace TradeControl.Web.Pages.Subject
{
    public class IndexModel : DI_BasePageModel
    {

        const string SessionKeyReturnUrl = "_returnUrlOrgIndex";
        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl);  }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        public IndexModel(NodeContext context) : base(context) { }

        public SelectList SubjectTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SubjectType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IList<Subject_vwSubjectLookup> Subject_SubjectLookup { get; set; }

        public async Task OnGetAsync(string returnUrl)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var orgTypes = from tb in NodeContext.Subject_tbTypes
                               orderby tb.SubjectType
                               select tb.SubjectType;

                SubjectTypes = new SelectList(await orgTypes.ToListAsync());

                var accounts = from tb in NodeContext.Subject_SubjectLookup
                               select tb;

                if (!string.IsNullOrEmpty(SubjectType))
                    accounts = accounts.Where(a => a.SubjectType == SubjectType);

                if (!string.IsNullOrEmpty(SearchString))
                    accounts = accounts.Where(a => a.SubjectName.Contains(SearchString));

                Subject_SubjectLookup = await accounts.OrderBy(a => a.SubjectName).ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
