using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Org.Contact
{
    [Authorize(Roles = "Administrators, Managers")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Org_vwContact Contact { get; set; }

        public async Task<IActionResult> OnGetAsync(string accountCode, string contactName)
        {
            if (accountCode == null || contactName == null)
                return NotFound();

            Contact = await NodeContext.Org_Contacts.FirstOrDefaultAsync(m => m.AccountCode == accountCode && m.ContactName == contactName);

            if (Contact == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAsync(string accountCode, string contactName)
        {
            try
            {
                if (accountCode == null || contactName == null)
                    return NotFound();

                Org_tbContact tbContact = await NodeContext.Org_tbContacts.Where(c => c.AccountCode == accountCode && c.ContactName == contactName).FirstOrDefaultAsync(); ;

                if (tbContact != null)
                {
                    NodeContext.Org_tbContacts.Remove(tbContact);
                    await NodeContext.SaveChangesAsync();
                }

                RouteValueDictionary route = new();
                route.Add("accountCode", accountCode);

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

