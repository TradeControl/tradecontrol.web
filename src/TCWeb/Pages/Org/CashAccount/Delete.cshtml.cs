using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Org.CashAccount
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        [BindProperty]
        public Org_vwCashAccount Org_CashAccount { get; set; }

        const string SessionKeyAccountType = "_accountType";

        string AccountType
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyAccountType);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyAccountType, value);
            }
        }

        public DeleteModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string cashAccountCode)
        {
            if (cashAccountCode == null)
                return NotFound();

            Org_CashAccount = await NodeContext.Org_CashAccounts.FirstOrDefaultAsync(m => m.CashAccountCode == cashAccountCode);
            
            if (Org_CashAccount == null)
                return NotFound();
            else
            {
                AccountType = Org_CashAccount.AccountType;

                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string cashAccountCode)
        {
            try
            {
                if (cashAccountCode == null)
                    return NotFound();

                var cashAccount = await NodeContext.Org_tbAccounts.FindAsync(cashAccountCode);

                if (cashAccount != null)
                {
                    NodeContext.Org_tbAccounts.Remove(cashAccount);
                    await NodeContext.SaveChangesAsync();
                }

                RouteValueDictionary route = new();
                route.Add("AccountType", AccountType);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
