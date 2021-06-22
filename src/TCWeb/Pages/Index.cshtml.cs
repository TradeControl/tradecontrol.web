using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        public IndexModel(NodeContext context
            ) : base(context)
        {
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

                    if (nodeSettings.IsFirstUse || ! nodeSettings.IsInitialised)
                        return RedirectToPage("/Admin/Setup/Config");
                    else
                        throw new Exception("Initialisation error");
                }
                else
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
