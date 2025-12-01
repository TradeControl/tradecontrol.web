using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Company
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Cash_vwTaxCorpTotal> Cash_CorpTaxTotals { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 50;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                var query = NodeContext.Cash_TaxCorpTotals
                                       .Where(t => t.StartOn <= DateTime.Today)
                                       .OrderByDescending(t => t.StartOn)
                                       .AsQueryable();

                // Page size options
                PageSizeOptions = new SelectList(new[] { "10", "50", "100" }, PageSize.ToString());

                // protect PageSize
                if (PageSize <= 0) PageSize = 50;

                // total count before paging
                TotalItems = await query.CountAsync();

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                Cash_CorpTaxTotals = await query
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
