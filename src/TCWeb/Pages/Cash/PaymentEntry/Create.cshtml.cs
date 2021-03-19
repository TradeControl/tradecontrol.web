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
    public class CreateModel : PageModel
    {
        private readonly TradeControl.Web.Data.NodeContext _context;

        public CreateModel(TradeControl.Web.Data.NodeContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

        public SelectList CashAccountCodes { get; set; }
        public SelectList AccountCodes { get; set; }
        public SelectList CashCodes { get; set; }
        public SelectList TaxCodes { get; set; }

        public async Task<IActionResult> OnGetAsync(string id, string mode)
        {

            var cashAccountCodes = from t in _context.Org_tbAccounts
                               where !t.AccountClosed && t.AccountTypeCode < 2 && t.CoinTypeCode == 2
                               orderby t.CashAccountCode
                               select t.CashAccountCode;

            CashAccountCodes = new SelectList(await cashAccountCodes.ToListAsync());

            var accountCodes = from t in _context.Org_AccountLookup
                              orderby t.AccountCode
                              select t.AccountCode;

            AccountCodes = new SelectList(await accountCodes.ToListAsync());

            if (mode != "0")
            {
                var cashCodes = from t in _context.Cash_CodeLookup
                                where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                orderby t.CashCode
                                select t.CashCode;

                CashCodes = new SelectList(await cashCodes.ToListAsync());

                var taxCodes = from t in _context.App_TaxCodes
                               orderby t.TaxCode
                               select t.TaxCode;

                TaxCodes = new SelectList(await taxCodes.ToListAsync());
            }


            Cash_PaymentsUnposted = new Cash_vwPaymentsUnposted();
            Cash_PaymentsUnposted.CashAccountCode = id;
            Cash_PaymentsUnposted.PaidOn = DateTime.Today;

            Profile profile = new Profile(_context);
            Cash_PaymentsUnposted.UserId = await profile.UserId;
            Cash_PaymentsUnposted.UpdatedBy = await profile.SqlUserName;
            Cash_PaymentsUnposted.InsertedBy = Cash_PaymentsUnposted.UpdatedBy;

            Cash_PaymentsUnposted.IsProfitAndLoss = false;

            return Page();
        }



        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            CashAccounts cashAccounts = new CashAccounts(_context);
            Cash_PaymentsUnposted.PaymentCode = await cashAccounts.NextPaymentCode();

            Cash_PaymentsUnposted.UpdatedOn = DateTime.Now;
            Cash_PaymentsUnposted.InsertedOn = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return Page();
            }


            _context.Cash_PaymentsUnposted.Add(Cash_PaymentsUnposted);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
