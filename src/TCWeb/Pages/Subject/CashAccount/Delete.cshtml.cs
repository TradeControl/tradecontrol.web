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

namespace TradeControl.Web.Pages.Subject.CashAccount
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_vwCashAccount Subject_CashAccount { get; set; }

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

        public DeleteModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string cashAccountCode)
        {
            if (cashAccountCode == null)
                return NotFound();

            Subject_CashAccount = await NodeContext.Subject_CashAccounts.FirstOrDefaultAsync(m => m.CashAccountCode == cashAccountCode);
            
            if (Subject_CashAccount == null)
                return NotFound();
            else
            {
                AccountType = Subject_CashAccount.AccountType;

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

                var cashAccount = await NodeContext.Subject_tbAccounts.FindAsync(cashAccountCode);

                if (cashAccount != null)
                {
                    NodeContext.Subject_tbAccounts.Remove(cashAccount);
                    await NodeContext.SaveChangesAsync();
                }

                RouteValueDictionary route = new();
                route.Add("AccountType", AccountType);

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
