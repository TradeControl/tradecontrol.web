using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.PaymentEntry
{
    public class EditModel : DI_BasePageModel
    {

        public EditModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public SelectList CashCodes { get; set; }
        public SelectList TaxCodes { get; set; }

        [BindProperty]
        public Cash_vwPaymentsUnposted Cash_PaymentsUnposted { get; set; }

        #region session data
        const string SessionKeyPaymentCode = "_paymentCode";
        const string SessionKeyCashCode = "_cashCode";
        const string SessionKeyTaxCode = "_taxCode";

        string PaymentCode
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyPaymentCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyPaymentCode, value);
            }
        }

        string CashCode
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyCashCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyCashCode, value);
            }
        }

        string TaxCode
        {
            get
            {
                try
                {
                    return HttpContext.Session.GetString(SessionKeyTaxCode);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Session.SetString(SessionKeyTaxCode, value);
            }
        }
        #endregion

        public async Task<IActionResult> OnGetAsync(string id, string cashcode, string taxcode)
        {
            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(PaymentCode))
                return NotFound();
            else if (!string.IsNullOrEmpty(id))
            {
                PaymentCode = id;
                CashCode = string.Empty;
                TaxCode = string.Empty;
            }

            Cash_PaymentsUnposted = await NodeContext.Cash_PaymentsUnposted.FirstOrDefaultAsync(m => m.PaymentCode == PaymentCode);

            if (Cash_PaymentsUnposted == null)
            {
                return NotFound();
            }

            if (Cash_PaymentsUnposted.CashCode != null)
            {
                var cashCodes = from t in NodeContext.Cash_CodeLookup
                                where t.CashTypeCode < (short)NodeEnum.CashType.Bank
                                orderby t.CashCode
                                select t.CashCode;

                CashCodes = new SelectList(await cashCodes.Distinct().ToListAsync());

                var taxCodes = from t in NodeContext.App_TaxCodes
                               orderby t.TaxCode
                               select t.TaxCode;

                TaxCodes = new SelectList(await taxCodes.Distinct().ToListAsync());
            }

            if (!string.IsNullOrEmpty(cashcode))
                CashCode = cashcode;

            if (!string.IsNullOrEmpty(taxcode))
                TaxCode = taxcode;

            if (!string.IsNullOrEmpty(CashCode))
                Cash_PaymentsUnposted.CashCode = CashCode;

            if (!string.IsNullOrEmpty(TaxCode))
                Cash_PaymentsUnposted.TaxCode = TaxCode;

            await SetViewData();

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            NodeContext.Attach(Cash_PaymentsUnposted).State = EntityState.Modified;

            try
            {
                await NodeContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Cash_vwPaymentsUnpostedExists(Cash_PaymentsUnposted.PaymentCode))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool Cash_vwPaymentsUnpostedExists(string id)
        {
            return NodeContext.Cash_PaymentsUnposted.Any(e => e.PaymentCode == id);
        }
    }
}
