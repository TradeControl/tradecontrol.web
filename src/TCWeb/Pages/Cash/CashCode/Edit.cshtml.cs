using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace TradeControl.Web.Pages.Cash.CashCode
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Cash_tbCode Cash_tbCode { get; set; }

        [BindProperty]
        public bool IsEnabled { get; set; }
        [BindProperty]
        public string Category { get; set; }
        [BindProperty]
        public string TaxDescription { get; set; }

        public SelectList Categories { get; set; }
        public SelectList TaxDescriptions { get; set; }


        public EditModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        public async Task<IActionResult> OnGetAsync(string cashCode)
        {
            try
            {
                if (string.IsNullOrEmpty(cashCode))
                    return NotFound();

                Cash_tbCode = await NodeContext.Cash_tbCodes.FindAsync(cashCode);

                if (Cash_tbCode == null)
                    return NotFound();

                IsEnabled = Cash_tbCode.IsEnabled != 0;
                TaxDescriptions = new SelectList(await NodeContext.App_TaxCodes.OrderBy(t => t.TaxCode).Select(t => t.TaxDescription).ToListAsync());
                Categories = new SelectList(await NodeContext.Cash_tbCategories
                                                            .Where(t => t.CategoryTypeCode == (short)NodeEnum.CategoryType.CashCode)
                                                            .OrderBy(t => t.Category)
                                                            .Select(t => t.Category)
                                                            .ToListAsync());

                TaxDescription = await NodeContext.App_tbTaxCodes.Where(t => t.TaxCode == Cash_tbCode.TaxCode).Select(t => t.TaxDescription).FirstAsync();
                Category = await NodeContext.Cash_tbCategories.Where(c => c.CategoryCode == Cash_tbCode.CategoryCode).Select(c => c.Category).FirstAsync();

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

                Cash_tbCode.IsEnabled = (short)(IsEnabled ? 1 : 0);
                Cash_tbCode.TaxCode = await NodeContext.App_tbTaxCodes.Where(t => t.TaxDescription == TaxDescription).Select(t => t.TaxCode).FirstAsync();
                Cash_tbCode.CategoryCode = await NodeContext.Cash_tbCategories.Where(c => c.Category == Category).Select(c => c.CategoryCode).FirstAsync();

                Profile profile = new(NodeContext);
                Cash_tbCode.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));
                Cash_tbCode.UpdatedOn = DateTime.Now;

                NodeContext.Attach(Cash_tbCode).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Cash_tbCodes.AnyAsync(e => e.CashCode == Cash_tbCode.CashCode))
                        return NotFound();
                    else
                    {
                        NodeContext.ErrorLog(new DbUpdateConcurrencyException());
                        throw;
                    }
                }

                RouteValueDictionary route = new();
                route.Add("category", Category);

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
