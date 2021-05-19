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

namespace TradeControl.Web.Pages.Org.CashAccount
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_tbAccount Org_CashAccount { get; set; }

        [BindProperty]
        public string CashDescription { get; set; }
        public SelectList CashCodes { get; set; }

        [BindProperty]
        public string AccountType { get; set; }
        [BindProperty]
        public string OrganisationName { get; set; }

        public EditModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        public async Task<IActionResult> OnGetAsync(string cashAccountCode)
        {
            if (string.IsNullOrEmpty(cashAccountCode))
                return NotFound();

            Org_CashAccount = await NodeContext.Org_tbAccounts.FirstOrDefaultAsync(t => t.CashAccountCode == cashAccountCode);

            if (Org_CashAccount == null)
                return NotFound();

            var cashCodes = await (from tb in NodeContext.Cash_BankCashCodes
                                   orderby tb.CashDescription
                                   select tb.CashDescription).ToListAsync();

            cashCodes.Add(string.Empty);

            CashCodes = new SelectList(cashCodes);
            if (string.IsNullOrEmpty(Org_CashAccount.CashCode))
                CashDescription = string.Empty;
            else
                CashDescription = await NodeContext.Cash_tbCodes.Where(t => t.CashCode == Org_CashAccount.CashCode).Select(t => t.CashDescription).FirstAsync();

            AccountType = await NodeContext.Org_tbAccountTypes.Where(t => t.AccountTypeCode == Org_CashAccount.AccountTypeCode).Select(t => t.AccountType).FirstAsync();
            OrganisationName = await NodeContext.Org_tbOrgs.Where(t => t.AccountCode == Org_CashAccount.AccountCode).Select(t => t.AccountName).FirstAsync();

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Profile profile = new(NodeContext);
            Org_CashAccount.UpdatedBy = await profile.UserName(UserManager.GetUserId(User));
            if (!string.IsNullOrEmpty(CashDescription))
                Org_CashAccount.CashCode = await NodeContext.Cash_tbCodes.Where(t => t.CashDescription == CashDescription).Select(t => t.CashCode).FirstAsync();

            NodeContext.Attach(Org_CashAccount).State = EntityState.Modified;

            try
            {
                await NodeContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Org_tbAccountExists(Org_CashAccount.CashAccountCode))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            RouteValueDictionary route = new();
            route.Add("CashAccountCode", Org_CashAccount.CashAccountCode);

            return RedirectToPage("./Index", route);
        }

        private bool Org_tbAccountExists(string cashAccountCode)
        {
            return NodeContext.Org_tbAccounts.Any(e => e.CashAccountCode == cashAccountCode);
        }
    }
}
