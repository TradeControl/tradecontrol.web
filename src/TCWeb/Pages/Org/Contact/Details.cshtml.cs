using System;
using System.Collections.Generic;
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

namespace TradeControl.Web.Pages.Org.Contact
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

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
    }
}
