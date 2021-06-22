using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
namespace TradeControl.Web.Pages.Org.Enquiry
{
    public class InvoicesModel : DI_BasePageModel
    {
        [BindProperty]
        public IList<Invoice_vwRegister> Org_Invoices { get; set; }

        [BindProperty]
        public Org_vwAccountLookup Org_Account { get; set; }

        public InvoicesModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string accountCode)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode))
                    return NotFound();

                Org_Account = await NodeContext.Org_AccountLookup.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

                if (Org_Account == null)
                    return NotFound();

                var invoices = from tb in NodeContext.Invoice_Register
                               where tb.AccountCode == accountCode
                               orderby tb.InvoicedOn descending
                               select tb;

                Org_Invoices = await invoices.ToListAsync();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
