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

namespace TradeControl.Web.Pages.Subject.CashAccount
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_tbAccount Subject_CashAccount { get; set; }

        [BindProperty]
        public string CashDescription { get; set; }
        public SelectList CashCodes { get; set; }

        [BindProperty]
        public string AccountType { get; set; }
        [BindProperty]
        public string OrganisationName { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public EditModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string cashSubjectCode)
        {
            try
            {
                if (string.IsNullOrEmpty(cashSubjectCode))
                    return NotFound();

                Subject_CashAccount = await NodeContext.Subject_tbAccounts.FirstOrDefaultAsync(t => t.AccountCode == cashSubjectCode);

                if (Subject_CashAccount == null)
                    return NotFound();

                var cashCodes = await (from tb in NodeContext.Cash_BankCashCodes
                                       orderby tb.CashDescription
                                       select tb.CashDescription).ToListAsync();

                cashCodes.Add(string.Empty);

                CashCodes = new SelectList(cashCodes);
                if (string.IsNullOrEmpty(Subject_CashAccount.CashCode))
                    CashDescription = string.Empty;
                else
                    CashDescription = await NodeContext.Cash_tbCodes.Where(t => t.CashCode == Subject_CashAccount.CashCode).Select(t => t.CashDescription).FirstAsync();

                AccountType = await NodeContext.Subject_tbAccountTypes.Where(t => t.AccountTypeCode == Subject_CashAccount.AccountTypeCode).Select(t => t.AccountType).FirstAsync();
                OrganisationName = await NodeContext.Subject_tbSubjects.Where(t => t.SubjectCode == Subject_CashAccount.SubjectCode).Select(t => t.SubjectName).FirstAsync();

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
                Subject_CashAccount.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));
                if (!string.IsNullOrEmpty(CashDescription))
                    Subject_CashAccount.CashCode = await NodeContext.Cash_tbCodes.Where(t => t.CashDescription == CashDescription).Select(t => t.CashCode).FirstAsync();

                NodeContext.Attach(Subject_CashAccount).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Subject_tbAccounts.AnyAsync(e => e.AccountCode == Subject_CashAccount.AccountCode))
                        return NotFound();
                    else
                        throw;

                }

                RouteValueDictionary route = new();
                route.Add("AccountCode", Subject_CashAccount.AccountCode);

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
