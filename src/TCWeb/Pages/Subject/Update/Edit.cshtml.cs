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


namespace TradeControl.Web.Pages.Subject.Update
{
    [Authorize(Roles = "Administrators, Managers")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_tbSubject Subject_tbSubject { get; set; }

        [BindProperty]
        public string SubjectType { get; set; }

        public SelectList SubjectTypes { get; set; }

        [BindProperty]
        public string SubjectStatus { get; set; }
        public SelectList SubjectStatuses { get; set; }

        [BindProperty]
        public string TaxDescription { get; set; }
        public SelectList TaxCodes { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            try
            {
                Subject_tbSubject = await NodeContext.Subject_tbSubjects.FindAsync(accountCode);

                if (Subject_tbSubject == null)
                    return NotFound();
                else
                {
                    SubjectTypes = new SelectList(await NodeContext.Subject_tbTypes.OrderBy(t => t.SubjectTypeCode).Select(t => t.SubjectType).ToListAsync());
                    SubjectStatuses = new SelectList(await NodeContext.Subject_tbStatuses.OrderBy(t => t.SubjectStatusCode).Select(t => t.SubjectStatus).ToListAsync());
                    TaxCodes = new SelectList(await NodeContext.App_TaxCodes.OrderBy(t => t.TaxCode).Select(t => t.TaxDescription).ToListAsync());

                    TaxDescription = await NodeContext.App_TaxCodes.Where(t => t.TaxCode == Subject_tbSubject.TaxCode).Select(t => t.TaxDescription).FirstOrDefaultAsync();
                    SubjectType = await NodeContext.Subject_tbTypes.Where(t => t.SubjectTypeCode == Subject_tbSubject.SubjectTypeCode).Select(t => t.SubjectType).FirstAsync();
                    SubjectStatus = await NodeContext.Subject_tbStatuses.Where(t => t.SubjectStatusCode == Subject_tbSubject.SubjectStatusCode).Select(t => t.SubjectStatus).FirstAsync();

                    await SetViewData();
                    return Page();
                }
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

                Profile profile = new(NodeContext);
                Subject_tbSubject.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

                Subject_tbSubject.SubjectStatusCode = await NodeContext.Subject_tbStatuses.Where(t => t.SubjectStatus == SubjectStatus).Select(t => t.SubjectStatusCode).FirstAsync();
                Subject_tbSubject.SubjectTypeCode = await NodeContext.Subject_tbTypes.Where(t => t.SubjectType == SubjectType).Select(t => t.SubjectTypeCode).FirstAsync();
                Subject_tbSubject.TaxCode = await NodeContext.App_tbTaxCodes.Where(t => t.TaxDescription == TaxDescription).Select(t => t.TaxCode).FirstAsync();

                NodeContext.Attach(Subject_tbSubject).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Subject_tbSubjects.AnyAsync(e => e.AccountCode == Subject_tbSubject.AccountCode))
                        return NotFound();
                    else
                        throw;

                }

                RouteValueDictionary route = new();
                route.Add("accountCode", Subject_tbSubject.AccountCode);
                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
