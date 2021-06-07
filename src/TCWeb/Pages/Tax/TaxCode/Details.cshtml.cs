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


namespace TradeControl.Web.Pages.Tax.TaxCode
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public App_vwTaxCode App_TaxCode { get; set; }

        public async Task<IActionResult> OnGetAsync(string taxCode)
        {
            if (taxCode == null)
                return NotFound();

            App_TaxCode = await NodeContext.App_TaxCodes.FirstOrDefaultAsync(m => m.TaxCode == taxCode);

            if (App_TaxCode == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}
