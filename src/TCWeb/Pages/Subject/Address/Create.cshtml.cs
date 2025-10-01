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
    public class CreateModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_tbAddress Subject_tbAddress { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        [BindProperty]
        [Display(Name = "Admin Address?")]
        public bool IsAdminAddress { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode))
                    return NotFound();

                var subject = await NodeContext.Subject_tbSubjects.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

                if (subject == null)
                    return NotFound();
                else
                    AccountName = subject.AccountName;

                Profile profile = new(NodeContext);
                Subjects orgs = new(NodeContext, accountCode);

                Subject_tbAddress = new()
                {
                    AccountCode = accountCode,
                    AddressCode = await orgs.NextAddressCode(),
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User)),
                    InsertedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };

                Subject_tbAddress.UpdatedBy = Subject_tbAddress.InsertedBy;

                IsAdminAddress = !(await NodeContext.Subject_tbAddresses.Where(t => t.AccountCode == accountCode).AnyAsync());

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

                NodeContext.Subject_tbAddresses.Add(Subject_tbAddress);
                if (IsAdminAddress)
                {
                    Subject_tbSubject subject = await NodeContext.Subject_tbSubjects.FirstOrDefaultAsync(t => t.AccountCode == Subject_tbAddress.AccountCode);
                    subject.AddressCode = Subject_tbAddress.AddressCode;
                }

                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();
                route.Add("accountCode", Subject_tbAddress.AccountCode);

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
