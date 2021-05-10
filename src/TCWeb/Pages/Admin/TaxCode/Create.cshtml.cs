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

namespace TradeControl.Web.Pages.Admin.TaxCode
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        const string SessionKeyReturnUrl = "_returnUrlCashCodeCreate";

        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        [BindProperty]
        public App_tbTaxCode App_tbTaxCode { get; set; }

        [BindProperty]
        public string Rounding { get; set; }
        [BindProperty]
        public string TaxType { get; set; }

        public SelectList Roundings { get; set; }
        public SelectList TaxTypes { get; set; }

        public CreateModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl)
        {
            await SetViewData();

            if (!string.IsNullOrEmpty(returnUrl))
                ReturnUrl = returnUrl;

            var roundings = NodeContext.App_tbRoundings.OrderBy(r => r.RoundingCode).Select(r => r.Rounding);
            Roundings = new SelectList(await roundings.ToListAsync());
            Rounding = await roundings.FirstOrDefaultAsync();

            var taxtypes = NodeContext.App_TaxCodeTypes.OrderBy(t => t.TaxTypeCode).Select(t => t.TaxType);
            TaxTypes = new SelectList(await taxtypes.ToListAsync());
            TaxType = await roundings.FirstOrDefaultAsync();

            Profile profile = new(NodeContext);
            var userName = await profile.UserName(UserManager.GetUserId(User));

            App_tbTaxCode = new App_tbTaxCode()
            {
                RoundingCode = (short)NodeEnum.RoundingCode.Round,
                TaxTypeCode = (short)NodeEnum.TaxType.VAT,
                UpdatedBy = userName,
                UpdatedOn = DateTime.Now
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            App_tbTaxCode.RoundingCode = await NodeContext.App_tbRoundings.Where(r => r.Rounding == Rounding).Select(r => r.RoundingCode).FirstAsync();
            App_tbTaxCode.TaxTypeCode = await NodeContext.App_TaxCodeTypes.Where(t => t.TaxType == TaxType).Select(t => t.TaxTypeCode).FirstAsync();

            NodeContext.App_tbTaxCodes.Add(App_tbTaxCode);
            await NodeContext.SaveChangesAsync();

            if (!string.IsNullOrEmpty(ReturnUrl))
                return LocalRedirect($"{ReturnUrl}?taxcode={App_tbTaxCode.TaxCode}");
            else
                return RedirectToPage("./Index");
        }
    }
}
