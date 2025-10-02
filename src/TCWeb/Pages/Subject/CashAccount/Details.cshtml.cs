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

namespace TradeControl.Web.Pages.Subject.CashAccount
{
    public class DetailsModel : DI_BasePageModel
    {
        [BindProperty]
        public Subject_vwCashAccount Subject_CashAccount { get; set; }

        public DetailsModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string cashSubjectCode)
        {
            if (cashSubjectCode == null)
                return NotFound();

            Subject_CashAccount = await NodeContext.Subject_CashAccounts.FirstOrDefaultAsync(m => m.AccountCode == cashSubjectCode);

            if (Subject_CashAccount == null)
                return NotFound();
            else
            {

                await SetViewData();
                return Page();
            }

        }
    }
}
