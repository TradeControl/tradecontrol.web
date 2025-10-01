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
        public string CashMode { get; set; }
        public SelectList CashModes { get; set; }

        public async Task<IActionResult> OnGetAsync(short? SubjectTypeCode)
        {
            try
            {
                if (SubjectTypeCode == null)
                    return NotFound();

                Subject_tbType = await NodeContext.Subject_tbTypes.SingleOrDefaultAsync(t => t.SubjectTypeCode == SubjectTypeCode);

                if (Subject_tbType == null)
                    return NotFound();

                var modes = NodeContext.Cash_tbModes.OrderBy(m => m.CashModeCode).Select(m => m.CashMode);
                CashModes = new SelectList(await modes.ToListAsync());
                CashMode = await NodeContext.Cash_tbModes
                                            .Where(t => t.CashModeCode == Subject_tbType.CashModeCode)
                                            .Select(t => t.CashMode).FirstAsync();

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
                orgType.CashModeCode = await NodeContext.Cash_tbModes.Where(t => t.CashMode == CashMode).Select(t => t.CashModeCode).SingleAsync();
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
