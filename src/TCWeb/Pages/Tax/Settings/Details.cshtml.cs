using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;


namespace TradeControl.Web.Pages.Tax.Settings
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public Cash_vwTaxType App_TaxType { get; set; }

        public async Task<IActionResult> OnGetAsync(short? taxTypeCode)
        {
            if (taxTypeCode == null)
                return NotFound();

            App_TaxType = await NodeContext.App_TaxTypes.FirstOrDefaultAsync(m => m.TaxTypeCode == taxTypeCode);

            if (App_TaxType == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}
