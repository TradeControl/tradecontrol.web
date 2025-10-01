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

namespace TradeControl.Web.Pages.Subject.Enquiry
{
    public class DetailsModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_vwDatasheet Subject_Account { get; set; }

        [BindProperty]
        public IList<Subject_tbAddress> Subject_Addresses { get; set; }

        [BindProperty]
        public IList<Subject_tbContact> Subject_Contacts { get; set; }

        public DetailsModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            if (accountCode == null)
                return NotFound();

            Subject_Account = await NodeContext.Subject_Datasheet.FirstOrDefaultAsync(m => m.AccountCode == accountCode);

            if (Subject_Account == null)
                return NotFound();
            else
            {
                Subject_Contacts = await NodeContext.Subject_tbContacts.Where(t => t.AccountCode == accountCode).OrderBy(t => t.ContactName).ToListAsync();
                Subject_Addresses = await NodeContext.Subject_tbAddresses.Where(t => t.AccountCode == accountCode && t.AddressCode != Subject_Account.AddressCode).ToListAsync();

                await SetViewData();
                return Page();
            }

        }
    }
}
