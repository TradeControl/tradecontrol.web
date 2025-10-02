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

namespace TradeControl.Web.Pages.Subject.Address
{
    [Authorize(Roles = "Administrators, Managers")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_tbAddress Subject_tbAddress { get; set; }

        [BindProperty]
        public string SubjectName { get; set; }

        [BindProperty]
        [Display(Name = "Admin Address?")]
        public bool IsAdminAddress { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string addressCode)
        {
            try
            {
                if (string.IsNullOrEmpty(addressCode))
                    return NotFound();

                Subject_tbAddress = await NodeContext.Subject_tbAddresses.FirstOrDefaultAsync(t => t.AddressCode == addressCode);

                if (Subject_tbAddress == null)
                    return NotFound();

                var subject = await NodeContext.Subject_tbSubjects.FirstAsync(t => t.SubjectCode == Subject_tbAddress.SubjectCode);
                SubjectName = subject.SubjectName;
                IsAdminAddress = subject.AddressCode == addressCode;

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

                Profile profile = new(NodeContext);
                Subject_tbAddress.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));

                NodeContext.Attach(Subject_tbAddress).State = EntityState.Modified;

                if (IsAdminAddress)
                {
                    Subject_tbSubject subject = await NodeContext.Subject_tbSubjects.FirstOrDefaultAsync(t => t.SubjectCode == Subject_tbAddress.SubjectCode);
                    if (subject.AddressCode != Subject_tbAddress.AddressCode)
                    {
                        subject.AddressCode = Subject_tbAddress.AddressCode;
                        NodeContext.Attach(subject).State = EntityState.Modified;
                    }
                }

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Subject_tbAddresses.AnyAsync(e => e.AddressCode == Subject_tbAddress.AddressCode))
                        return NotFound();
                    else
                        throw;

                }

                RouteValueDictionary route = new();
                route.Add("accountCode", Subject_tbAddress.SubjectCode);

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
