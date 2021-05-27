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

        #region local model
        public class Cash_CategoriesList
        {
            [Required]
            [StringLength(10)]
            [Display(Name = "Category Code")]
            public string CategoryCode { get; set; }
            [StringLength(50)]
            [Display(Name = "Category")]
            public string Category { get; set; }
            [StringLength(20)]
            [Display(Name = "Cat. Type")]
            public string CategoryType { get; set; }
            [Display(Name = "Cash Type Code")]
            public short CashTypeCode { get; set; }
            [Display(Name = "Display Order")]
            public short DisplayOrder { get; set; }
            [StringLength(25)]
            [Display(Name = "Cash Type")]
            public string CashType { get; set; }
            [StringLength(10)]
            [Display(Name = "Mode")]
            public string CashMode { get; set; }
        }

        public IList<Cash_CategoriesList> Cash_Categories { get; set; }

        #endregion

        public SelectList CashTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CashType { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IndexModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) {}

        public async Task OnGetAsync(string returnUrl, string cashTypeCode)
        {
            await SetViewData();

            if (!string.IsNullOrEmpty(returnUrl))
                ReturnUrl = returnUrl;

            var cashTypes = from tb in NodeContext.Cash_tbTypes
                            orderby tb.CashType
                            select tb.CashType;

            CashTypes = new SelectList(await cashTypes.ToListAsync());

            IQueryable<Cash_CategoriesList> categories = from c in NodeContext.Cash_tbCategories
                                join p in NodeContext.Cash_tbModes on c.CashModeCode equals p.CashModeCode
                                join t in NodeContext.Cash_tbTypes on c.CashTypeCode equals t.CashTypeCode
                                join ct in NodeContext.Cash_tbCategoryTypes on c.CategoryTypeCode equals ct.CategoryTypeCode
                                where c.CategoryTypeCode == (short)NodeEnum.CategoryType.CashCode                              
                                select new Cash_CategoriesList
                                {
                                    CategoryCode = c.CategoryCode,
                                    Category = c.Category,
                                    CategoryType = ct.CategoryType,
                                    DisplayOrder = c.DisplayOrder,
                                    CashTypeCode = c.CashTypeCode,
                                    CashMode = p.CashMode,
                                    CashType = t.CashType
                                };

            if (!string.IsNullOrEmpty(cashTypeCode))
            {
                NodeEnum.CashType cashType = (NodeEnum.CashType)short.Parse(cashTypeCode);
                categories = from tb in categories
                             where tb.CashTypeCode == (short)cashType
                             select tb;
            }

            if (!string.IsNullOrEmpty(SearchString))
                categories = categories.Where(t => t.Category.Contains(SearchString));

            if (!string.IsNullOrEmpty(CashType))
                categories = categories.Where(t => t.CashType == CashType);           

            Cash_Categories = await categories.OrderBy(t => t.CashTypeCode).OrderBy(t => t.DisplayOrder).ToListAsync();

        }
    }
}
