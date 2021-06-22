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
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Settings
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<Cash_vwTaxType> App_TaxTypes { get; set; }

        [BindProperty]
        [Display(Name = "Net Profit Category")]
        public string NetProfitCode { get; set; }

        [BindProperty]
        [Display(Name = "Vat Categories")]
        public string VatCategoryCode { get; set; }

        public SelectList Categories { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await SetViewData();

                App_TaxTypes = await NodeContext.App_TaxTypes.OrderBy(t => t.TaxTypeCode).ToListAsync();

                Categories = new SelectList(await NodeContext.App_CandidateCategoryCodes.OrderBy(c => c.Category).Select(c => c.Category).ToListAsync());
                var options = await NodeContext.App_tbOptions.FirstAsync();

                NetProfitCode = await NodeContext.Cash_tbCategories
                                .Where(c => c.CategoryCode == options.NetProfitCode)
                                .Select(c => c.Category).FirstAsync();

                VatCategoryCode = await NodeContext.Cash_tbCategories
                                .Where(c => c.CategoryCode == options.VatCategoryCode)
                                .Select(c => c.Category).FirstAsync();

            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                var options = await NodeContext.App_tbOptions.FirstAsync();
                options.NetProfitCode = await NodeContext.Cash_tbCategories.Where(t => t.Category == NetProfitCode).Select(t => t.CategoryCode).FirstAsync();
                options.VatCategoryCode = await NodeContext.Cash_tbCategories.Where(t => t.Category == VatCategoryCode).Select(t => t.CategoryCode).FirstAsync();

                NodeContext.Attach(options).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.App_tbOptions.AnyAsync())
                        return NotFound();
                    else
                        throw;

                }

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}

