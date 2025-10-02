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
    public class StatementModel : DI_BasePageModel
    {
        [BindProperty]
        public IList<Subject_vwStatement> Subject_Statement { get; set; }

        [BindProperty]
        public Subject_vwSubjectLookup Subject_Account { get; set; }

        public StatementModel(NodeContext context) : base(context) {}

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode))
                    return NotFound();

                Subject_Account = await NodeContext.Subject_SubjectLookup.FirstOrDefaultAsync(t => t.SubjectCode == accountCode);

                if (Subject_Account == null)
                    return NotFound();

                var statement = from tb in NodeContext.Subject_Statement
                                where tb.SubjectCode == accountCode
                                orderby tb.RowNumber descending
                                select tb;

                Subject_Statement = await statement.ToListAsync();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }

        }
    }
}

