using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    public class IndexModel : DI_BasePageModel
    {
        const string SessionKeyReturnUrl = "_returnUrlCategoryCodeIndex";
        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        public IList<Cash_Category> Cash_Categories { get; set; }

        public SelectList CashTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CashType { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IndexModel(NodeContext context) : base(context) {}

        public async Task OnGetAsync(string returnUrl, short? cashTypeCode)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var cashTypes = from tb in NodeContext.Cash_tbTypes
                                orderby tb.CashType
                                select tb.CashType;

                CashTypes = new SelectList(await cashTypes.ToListAsync());

                IQueryable<Cash_Category> categories = from c in NodeContext.Cash_tbCategories
                                                             join p in NodeContext.Cash_tbModes on c.CashModeCode equals p.CashModeCode
                                                             join t in NodeContext.Cash_tbTypes on c.CashTypeCode equals t.CashTypeCode
                                                             join ct in NodeContext.Cash_tbCategoryTypes on c.CategoryTypeCode equals ct.CategoryTypeCode
                                                             where c.CategoryTypeCode == (short)NodeEnum.CategoryType.CashCode
                                                             select new Cash_Category
                                                             {
                                                                 CategoryCode = c.CategoryCode,
                                                                 Category = c.Category,
                                                                 CategoryType = ct.CategoryType,
                                                                 DisplayOrder = c.DisplayOrder,
                                                                 CashTypeCode = c.CashTypeCode,
                                                                 CashMode = p.CashMode,
                                                                 CashType = t.CashType,
                                                                 IsEnabled = c.IsEnabled == 0 ? false : true
                                                             };

                if (cashTypeCode != null)
                {
                    NodeEnum.CashType cashType = (NodeEnum.CashType)cashTypeCode;
                    categories = from tb in categories
                                 where tb.CashTypeCode == (short)cashType
                                 select tb;
                    CashType = await NodeContext.Cash_tbTypes.Where(t => t.CashTypeCode == cashTypeCode).Select(t => t.CashType).FirstOrDefaultAsync();
                }
                else if (!string.IsNullOrEmpty(CashType))
                    categories = categories.Where(t => t.CashType == CashType);

                if (!string.IsNullOrEmpty(SearchString))
                    categories = categories.Where(t => t.Category.Contains(SearchString));


                Cash_Categories = await categories.OrderBy(t => t.CashTypeCode).OrderBy(t => t.DisplayOrder).ToListAsync();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
