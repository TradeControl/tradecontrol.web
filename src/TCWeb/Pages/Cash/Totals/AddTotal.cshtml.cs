using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Totals
{
    public class AddTotalModel : DI_BasePageModel
    {
        public IList<Cash_vwCategoryTotalCandidate> Cash_Candidates { get; set; }
        public Cash_tbCategory Cash_Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryType { get; set; }
        public SelectList CategoryTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CashType { get; set; }
        public SelectList CashTypes { get; set; }

        const string SessionKeyCategoryCode = "_AddTotalCategoryCode";

        public string CategoryCode
        {
            get { return HttpContext.Session.GetString(SessionKeyCategoryCode); }
            set { HttpContext.Session.SetString(SessionKeyCategoryCode, value); }
        }

        public AddTotalModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string categoryCode)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryCode) && string.IsNullOrEmpty(CategoryCode))
                    return NotFound();
                else if (!string.IsNullOrEmpty(categoryCode))
                    CategoryCode = categoryCode;
                    

                Cash_Category = await NodeContext.Cash_tbCategories
                                .Where(c => c.CategoryCode == CategoryCode && c.CategoryTypeCode == (short)NodeEnum.CategoryType.CashTotal).FirstOrDefaultAsync();

                if (Cash_Category == null)
                    return NotFound();

                CategoryTypes = new SelectList(await NodeContext.Cash_tbCategoryTypes
                                        .Where(t => t.CategoryTypeCode < (short)NodeEnum.CategoryType.Expression)
                                        .Select(t => t.CategoryType).ToListAsync());

                CashTypes = new SelectList(await NodeContext.Cash_tbTypes
                                        .OrderBy(t => t.CashTypeCode)
                                        .Select(t => t.CashType).ToListAsync());

                var candidates = from tb in NodeContext.Cash_CategoryTotalCandidates
                                 where tb.CategoryCode != Cash_Category.CategoryCode
                                 select tb;

                if (!string.IsNullOrEmpty(CategoryType))
                    candidates = candidates.Where(t => t.CategoryType == CategoryType);

                if (!string.IsNullOrEmpty(CashType))
                    candidates = candidates.Where(t => t.CashType == CashType);

                candidates = from tb in candidates
                             orderby tb.CategoryType, tb.CashType, tb.CashPolarity
                             select tb;

                Cash_Candidates = await candidates.ToListAsync();

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