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

namespace TradeControl.Web.Pages.Tax.TaxCode
{
    public class SearchModel : DI_BasePageModel
    {
        const string SessionKeyReturnUrl = "_returnUrlTaxCodeIndex";

        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        public SelectList TaxTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TaxType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public SearchModel(NodeContext context) : base(context) { }

        public IList<App_vwTaxCode> App_TaxCodes { get;set; }

        public async Task OnGetAsync(string returnUrl)
        {
            try
            {
                await SetViewData();

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                var taxtypes = from tb in NodeContext.App_TaxCodeTypes
                               orderby tb.TaxType
                               select tb.TaxType;

                TaxTypes = new SelectList(await taxtypes.ToListAsync());

                var taxCodes = from tb in NodeContext.App_TaxCodes
                                select tb;

                if (!string.IsNullOrEmpty(TaxType))
                    taxCodes = taxCodes.Where(t => t.TaxType == TaxType);

                if (!string.IsNullOrEmpty(SearchString))
                    taxCodes = taxCodes.Where(t => t.TaxDescription.Contains(SearchString));

                App_TaxCodes = await taxCodes.ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
