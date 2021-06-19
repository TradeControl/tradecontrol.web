using System;
using System.Collections.Generic;
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

namespace TradeControl.Web.Pages.Cash.CashCode
{
    public class IndexModel : DI_BasePageModel
    {
        const string SessionKeyReturnUrl = "_returnUrlCashCodeIndex";
        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        public SelectList Categories { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IList<Cash_vwCode> Cash_Codes { get; set; }

        public IndexModel(NodeContext context) : base(context) { }

        public async Task OnGetAsync(string returnUrl)
        {
            try
            {
                await SetViewData();

                var categories = from tb in NodeContext.Cash_tbCategories
                                 where tb.CategoryTypeCode == (short)NodeEnum.CategoryType.CashCode
                                 select tb;
                

                var cashcodes = from tb in NodeContext.Cash_Codes select tb;

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    ReturnUrl = returnUrl;
                    cashcodes = cashcodes.Where(t => t.CashTypeCode < (short)NodeEnum.CashType.Bank && t.IsCashEnabled && t.IsCategoryEnabled);
                    categories = categories.Where(t => t.CashTypeCode == (short)NodeEnum.CashType.Trade);
                }

                Categories = new SelectList(await categories.OrderBy(t => t.Category).Select(t => t.Category).ToListAsync());

                if (!string.IsNullOrEmpty(Category))
                    cashcodes = cashcodes.Where(t => t.Category == Category);

                if (!string.IsNullOrEmpty(SearchString))
                    cashcodes = cashcodes.Where(t => t.CashDescription.Contains(SearchString));

                Cash_Codes = await cashcodes.OrderBy(t => t.CashCode).ToListAsync();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
