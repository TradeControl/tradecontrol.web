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

        public IList<Cash_vwCodeLookup> Cash_CodeLookup { get; set; }

        public IndexModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<Cash_vwCodeLookup> Cash_vwCodeLookup { get;set; }

        public async Task OnGetAsync(string returnUrl)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var categories = from tb in NodeContext.Cash_CategoryTrades
                                 orderby tb.Category
                                 select tb.Category;

                Categories = new SelectList(await categories.ToListAsync());

                var cashcodes = NodeContext.Cash_CodeLookup.Where(t => t.CashTypeCode < (short)NodeEnum.CashType.Bank);

                if (!string.IsNullOrEmpty(Category))
                    cashcodes = cashcodes.Where(t => t.Category == Category);

                if (!string.IsNullOrEmpty(SearchString))
                    cashcodes = cashcodes.Where(t => t.CashDescription.Contains(SearchString));

                Cash_CodeLookup = await cashcodes.OrderBy(t => t.CashCode).ToListAsync();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
