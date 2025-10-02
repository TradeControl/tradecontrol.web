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

namespace TradeControl.Web.Pages.Invoice.Raise
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Invoice_vwEntry Entry { get; set; }

        public async Task<IActionResult> OnGetAsync(string accountCode, string cashCode)
        {
            if (accountCode == null || cashCode == null)
                return NotFound();

            Entry = await NodeContext.Invoice_Entries.FirstOrDefaultAsync(m => m.SubjectCode == accountCode && m.CashCode == cashCode);

            if (Entry == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }

        }
    }
}
