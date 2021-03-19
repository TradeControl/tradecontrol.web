using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class IndexModel : PageModel
    {
        private readonly TradeControl.Web.Data.NodeContext _context;

        public IndexModel(TradeControl.Web.Data.NodeContext context)
        {
            _context = context;            
        }

        public SelectList CashAccounts { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CashAccount { get; set; }

        public IList<Cash_vwPaymentsUnposted> Cash_PaymentsUnposted { get;set; }


        public async Task OnGetAsync()
        {
            if (CashAccount == null)
                CashAccount = await _context.CurrentAccount;

            var cashAccounts = from t in _context.Org_tbAccounts
                               where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                               orderby t.CashAccountCode
                               select t.CashAccountCode;

            CashAccounts = new SelectList(await cashAccounts.ToListAsync());

            if (!string.IsNullOrEmpty(CashAccount))
                Cash_PaymentsUnposted = await _context.Cash_PaymentsUnposted.Where(t => t.CashAccountCode == CashAccount).ToListAsync();
            else
                Cash_PaymentsUnposted = await _context.Cash_PaymentsUnposted.Select(t => t).ToListAsync();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            CashAccounts cashAccount = new CashAccounts(_context);
            if (await cashAccount.Post())
                return RedirectToPage("../../Index");
            else
                return RedirectToPage("../../Index");  //Error Log Page
        }
   }
}
