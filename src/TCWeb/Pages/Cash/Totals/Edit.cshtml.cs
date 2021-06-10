using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Totals
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public Cash_tbCategory Cash_tbCategory { get; set; }

        public IList<string> Child_Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(string categoryCode, string remove, string childCode)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryCode))
                    return NotFound();

                Cash_tbCategory = await NodeContext.Cash_tbCategories.FindAsync(categoryCode);

                if (Cash_tbCategory == null)
                    return NotFound();


                if (!string.IsNullOrEmpty(childCode))
                {                    
                    if (!await NodeContext.Cash_tbCategoryTotals.Where(c => c.ParentCode == categoryCode && c.ChildCode == childCode).AnyAsync())
                    {                        
                        var total = new Cash_tbCategoryTotal()
                        {
                            ParentCode = categoryCode,
                            ChildCode = childCode
                        };

                        NodeContext.Cash_tbCategoryTotals.Add(total);
                        await NodeContext.SaveChangesAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(remove))
                {
                    childCode = await NodeContext.Cash_tbCategories.Where(c => c.Category == remove).Select(c => c.CategoryCode).FirstOrDefaultAsync();
                    var total = await NodeContext.Cash_tbCategoryTotals.Where(t => t.ParentCode == categoryCode && t.ChildCode == childCode).FirstOrDefaultAsync();

                    if (total != null)
                    {
                        NodeContext.Cash_tbCategoryTotals.Remove(total);
                        await NodeContext.SaveChangesAsync();
                    }
                }

                var child_totals = from totals in NodeContext.Cash_tbCategoryTotals
                                   join cat_total in NodeContext.Cash_tbCategories on totals.ChildCode equals cat_total.CategoryCode
                                   where totals.ParentCode == Cash_tbCategory.CategoryCode
                                   orderby cat_total.Category
                                   select cat_total.Category;

                Child_Categories = await child_totals.ToListAsync();

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
                        return base.NotFound();
                    else
                    {
                        NodeContext.ErrorLog(new DbUpdateConcurrencyException());
                        throw;
                    }
                }

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
