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
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Org.Type
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        [BindProperty]
        public Org_tbType Org_tbType { get; set; }

        [BindProperty]
        public string CashMode { get; set; }
        public SelectList CashModes { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var orgTypeCode = (await NodeContext.Org_tbTypes.MaxAsync(t => t.OrganisationTypeCode)) + 1;

                Org_tbType = new()
                {
                    OrganisationTypeCode = (short)orgTypeCode,
                    CashModeCode = (short)NodeEnum.CashMode.Income
                };

                var modes = NodeContext.Cash_tbModes.OrderBy(m => m.CashModeCode).Select(m => m.CashMode);
                CashModes = new SelectList(await modes.ToListAsync());
                CashMode = await NodeContext.Cash_tbModes
                                            .Where(t => t.CashModeCode == Org_tbType.CashModeCode)
                                            .Select(t => t.CashMode).FirstAsync();
                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Org_tbType.CashModeCode = await NodeContext.Cash_tbModes.Where(t => t.CashMode == CashMode).Select(t => t.CashModeCode).FirstAsync();
                NodeContext.Org_tbTypes.Add(Org_tbType);
                await NodeContext.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
