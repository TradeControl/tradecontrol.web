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

namespace TradeControl.Web.Pages.Subject.Type
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Subject_tbType Subject_tbType { get; set; }

        [BindProperty]
        public string CashPolarity { get; set; }
        public SelectList CashPolaritys { get; set; }

        public async Task<IActionResult> OnGetAsync(short? SubjectTypeCode)
        {
            try
            {
                if (SubjectTypeCode == null)
                    return NotFound();

                Subject_tbType = await NodeContext.Subject_tbTypes.SingleOrDefaultAsync(t => t.SubjectTypeCode == SubjectTypeCode);

                if (Subject_tbType == null)
                    return NotFound();

                var modes = NodeContext.Cash_tbPolaritys.OrderBy(m => m.CashPolarityCode).Select(m => m.CashPolarity);
                CashPolaritys = new SelectList(await modes.ToListAsync());
                CashPolarity = await NodeContext.Cash_tbPolaritys
                                            .Where(t => t.CashPolarityCode == Subject_tbType.CashPolarityCode)
                                            .Select(t => t.CashPolarity).FirstAsync();

                await SetViewData();
                return Page();
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

                var orgType = await NodeContext.Subject_tbTypes.SingleOrDefaultAsync(t => t.SubjectTypeCode == Subject_tbType.SubjectTypeCode);
                orgType.CashPolarityCode = await NodeContext.Cash_tbPolaritys.Where(t => t.CashPolarity == CashPolarity).Select(t => t.CashPolarityCode).SingleAsync();
                orgType.SubjectType = Subject_tbType.SubjectType;

                NodeContext.Attach(orgType).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Subject_tbTypes.AnyAsync(e => e.SubjectTypeCode == Subject_tbType.SubjectTypeCode))
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
