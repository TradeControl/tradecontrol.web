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
    public class PaymentsModel : DI_BasePageModel
    {
        [BindProperty]
        public IList<Cash_vwPayment> Subject_Payments { get; set; }

        [BindProperty]
        public Subject_vwSubjectLookup Subject_Account { get; set; }

        public PaymentsModel(NodeContext context) : base(context) { }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 50; // default 50

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode))
                    return NotFound();

                Subject_Account = await NodeContext.Subject_SubjectLookup.FirstOrDefaultAsync(t => t.SubjectCode == accountCode);

                if (Subject_Account == null)
                    return NotFound();

                // Base query
                IQueryable<Cash_vwPayment> payments = from tb in NodeContext.Cash_Payments
                                                      where tb.SubjectCode == accountCode
                                                      select tb;

                // Page size options (10, 50, 100)
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());
                if (PageSize <= 0) PageSize = 50;

                // compute totals BEFORE paging
                TotalItems = await payments.CountAsync();

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                Subject_Payments = await payments
                    .OrderByDescending(p => p.PaidOn)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

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
