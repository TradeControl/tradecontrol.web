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
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Transfer
{
    public class CreateModel : DI_BasePageModel
    {
        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        [BindProperty]
        public Cash_vwTransfersUnposted Cash_TransfersUnposted { get; set; }

        public SelectList CashAccounts { get; set; }
        public SelectList CashCodes { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        [BindProperty]
        public string CashDescription { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var cashAccountList = from tb in NodeContext.Subject_tbAccounts
                                      where !tb.AccountClosed && tb.AccountTypeCode == (short)NodeEnum.CashAccountType.Cash && tb.CoinTypeCode == (short)NodeEnum.CoinType.Fiat
                                      select tb.AccountName;

                CashAccounts = new SelectList(await cashAccountList.ToListAsync());

                CashAccounts cashAccounts = new(NodeContext);
                string cashSubjectCode = await cashAccounts.CurrentAccount();
                AccountName = await NodeContext.Subject_tbAccounts.Where(t => t.AccountCode == cashSubjectCode).Select(t => t.AccountName).FirstOrDefaultAsync();

                var cashCodeList = from tb in NodeContext.Cash_TransferCodeLookup
                                   orderby tb.CashCode
                                   select tb.CashDescription;

                CashCodes = new SelectList(await cashCodeList.ToListAsync());
                CashDescription = await cashCodeList.FirstOrDefaultAsync();

                Profile profile = new(NodeContext);
                string cashCode = await NodeContext.Cash_tbCodes.Where(t => t.CashDescription == CashDescription).Select(t => t.CashCode).FirstOrDefaultAsync();
                CashCodes cashCodes = new(NodeContext, cashCode);

                Cash_TransfersUnposted = new Cash_vwTransfersUnposted
                {
                    AccountCode = cashSubjectCode,
                    CashCode = cashCodes.CashCode,
                    TaxCode = cashCodes.TaxCode,
                    PaymentCode = await cashAccounts.NextPaymentCode(),
                    SubjectCode = await profile.CompanySubjectCode(),
                    PaidOn = DateTime.Today,
                    UserId = await profile.UserId(UserManager.GetUserId(User)),
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User))
                };

                Cash_TransfersUnposted.UpdatedBy = Cash_TransfersUnposted.InsertedBy;

                await SetViewData();

                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                Cash_TransfersUnposted.AccountCode = await NodeContext.Subject_tbAccounts.Where(t => t.AccountName == AccountName).Select(t => t.AccountCode).FirstOrDefaultAsync();
                Cash_TransfersUnposted.CashCode = await NodeContext.Cash_tbCodes.Where(t => t.CashDescription == CashDescription).Select(t => t.CashCode).FirstOrDefaultAsync();

                CashCodes cashCode = new(NodeContext, Cash_TransfersUnposted.CashCode);
                Cash_TransfersUnposted.TaxCode = cashCode.TaxCode;

                Cash_TransfersUnposted.UpdatedOn = DateTime.Now;
                Cash_TransfersUnposted.InsertedOn = DateTime.Now;

                if (!ModelState.IsValid || (Cash_TransfersUnposted.PaidInValue + Cash_TransfersUnposted.PaidOutValue == 0))
                    return Page();

                NodeContext.Cash_TransfersUnposted.Add(Cash_TransfersUnposted);
                await NodeContext.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
