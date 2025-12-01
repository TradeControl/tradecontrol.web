using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Type
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

        public Subject_vwTypeLookup Subject_Type { get; set; }

        [BindProperty]
        [Display(Name ="Accounts")]
        public int NumberOfAccounts { get; set; }

        public async Task<IActionResult> OnGetAsync(short? SubjectTypeCode)
        {
            try
            {
                if (SubjectTypeCode == null)
                    return NotFound();

                Subject_Type = await NodeContext.Subject_TypeLookup.FirstOrDefaultAsync(t => t.SubjectTypeCode == SubjectTypeCode);

                if (Subject_Type == null)
                    return NotFound();

                NumberOfAccounts = await NodeContext.Subject_tbSubjects.Where(o => o.SubjectTypeCode == SubjectTypeCode).CountAsync();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(short? SubjectTypeCode)
        {
            try
            {
                if (SubjectTypeCode == null)
                    return NotFound();

                var tbSubjectType = await NodeContext.Subject_tbTypes.FindAsync(SubjectTypeCode);
                NodeContext.Subject_tbTypes.Remove(tbSubjectType);
                await NodeContext.SaveChangesAsync();

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
