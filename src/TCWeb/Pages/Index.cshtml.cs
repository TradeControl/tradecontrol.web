using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages
{
    [AllowAnonymous]
    public class IndexModel : DI_BasePageModel
    {
        [BindProperty]
        public App_vwIdentity App_Identity { get; set; }

        [BindProperty]
        [Display(Name = "Current Period")]
        public string CurrentPeriod { get; set; }


        [BindProperty]
        [Display(Name = "Web Version")]
        public string WebVersion { get; set; }

        [BindProperty]
        [Display(Name = "Node Version")]
        public string SqlNodeVersion { get; set; }

        IConfiguration Configuration { get; }

        public IndexModel(NodeContext context, IConfiguration configuration
            ) : base(context)
        {
            Configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await SetViewData();

                App_Identity = await NodeContext.App_Identity.OrderBy(i => i.UserName).SingleOrDefaultAsync();

                if (App_Identity == null)
                {
                    NodeSettings nodeSettings = new NodeSettings(NodeContext);

                    if (nodeSettings.IsFirstUse)
                        await NodeContext.InitializeNode();
                    
                    if (!nodeSettings.IsInitialised)
                        return RedirectToPage("/Admin/Setup/Config");
                    else
                        throw new Exception("Initialisation error");
                }
                else
                {
                    FinancialPeriods periods = new(NodeContext);
                    CurrentPeriod = $"{periods.ActiveYearDesc}-{periods.ActiveMonthName}";
                    SqlNodeVersion = Configuration.GetSection("Settings")["SqlNodeVersion"];
                    WebVersion = Configuration.GetSection("Settings")["WebVersion"];
                    return Page();
                }

            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
