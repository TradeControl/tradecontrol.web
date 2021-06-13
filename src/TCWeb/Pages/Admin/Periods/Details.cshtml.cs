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

namespace TradeControl.Web.Pages.Admin.Periods
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }
        
        public App_vwYear App_Year { get; set; }

        public IList<App_vwYearPeriod> App_YearPeriods { get; set; }
            
        public async Task<IActionResult> OnGetAsync(short? yearNumber)
        {
            try
            {
                if (yearNumber == null)
                    return NotFound();

                App_Year = await NodeContext.App_Years.Where(y => y.YearNumber == yearNumber).FirstOrDefaultAsync();

                if (App_Year == null)
                    return NotFound();

                App_YearPeriods = await NodeContext.App_YearPeriods
                                    .Where(p => p.YearNumber == yearNumber)
                                    .OrderBy(p => p.StartOn)
                                    .ToListAsync();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }

        }

    }
}
