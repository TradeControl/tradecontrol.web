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

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        [BindProperty]
        public Cash_tbCategory Cash_tbCategory { get; set; }

        public SelectList CashModes { get; set; }
        public SelectList CashTypes { get; set; }

        [BindProperty]
        public string CashMode { get; set; }
        [BindProperty]
        public string CashType { get; set; }
        [BindProperty]
        public bool IsEnabled { get; set; }

        public async Task<IActionResult> OnGetAsync(string categoryCode)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryCode))
                    return NotFound();

                Cash_tbCategory = await NodeContext.Cash_tbCategories.FindAsync(categoryCode);

                if (Cash_tbCategory == null)
                    return NotFound();

                IsEnabled = Cash_tbCategory.IsEnabled != 0;

                var modes = NodeContext.Cash_tbModes.OrderBy(m => m.CashModeCode).Select(m => m.CashMode);
                CashModes = new SelectList(await modes.ToListAsync());
                CashMode = await NodeContext.Cash_tbModes.Where(m => m.CashModeCode == Cash_tbCategory.CashModeCode).Select(m => m.CashMode).FirstAsync();

                var types = NodeContext.Cash_tbTypes.OrderBy(t => t.CashTypeCode).Select(t => t.CashType);
                CashTypes = new SelectList(await types.ToListAsync());
                CashType = await NodeContext.Cash_tbTypes.Where( t => t.CashTypeCode == Cash_tbCategory.CashTypeCode).Select(t => t.CashType).FirstAsync();
               
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

                Cash_tbCategory.IsEnabled = (short)(IsEnabled ? 1 : 0);
                Cash_tbCategory.CashTypeCode = await NodeContext.Cash_tbTypes.Where(t => t.CashType == CashType).Select(t => t.CashTypeCode).FirstAsync();
                Cash_tbCategory.CashModeCode = await NodeContext.Cash_tbModes.Where(m => m.CashMode == CashMode).Select(m => m.CashModeCode).FirstAsync();

                Profile profile = new(NodeContext);
                Cash_tbCategory.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));
                Cash_tbCategory.UpdatedOn = DateTime.Now;

                NodeContext.Attach(Cash_tbCategory).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Cash_tbCategories.AnyAsync(e => e.CategoryCode == Cash_tbCategory.CategoryCode))
                        return NotFound();
                    else
                        throw;
                }

                RouteValueDictionary route = new();
                route.Add("cashTypeCode", Cash_tbCategory.CashTypeCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
