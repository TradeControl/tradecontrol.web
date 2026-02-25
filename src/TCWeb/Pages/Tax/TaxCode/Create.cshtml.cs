using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.TaxCode
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

        [BindProperty]
        [Display(Name = "Tax Rate (%)")]
        public decimal TaxRatePercent { get; set; }

        public SelectList Roundings { get; set; }
        public SelectList TaxTypes { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl, string taxType, string searchString)
        {
            try
            {
                await LoadListsAsync(taxType);

                if (!string.IsNullOrEmpty(returnUrl))
                    ReturnUrl = returnUrl;

                App_tbTaxCode = new App_tbTaxCode()
                {
                    RoundingCode = (short)NodeEnum.RoundingCode.Round,
                    TaxTypeCode = (short)NodeEnum.TaxType.VAT,
                    Decimals = 2,
                    TaxRate = 0m
                };

                TaxRatePercent = App_tbTaxCode.TaxRate * 100m;

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        // Same pattern as Admin.Users.CreateModel.OnGetDefaultUserIdAsync
        public async Task<JsonResult> OnGetDefaultTaxCodeAsync(string description)
        {
            try
            {
                description ??= string.Empty;
                description = description.Trim();

                if (string.IsNullOrWhiteSpace(description))
                    return new JsonResult(new { ok = true, taxCode = string.Empty });

                // Uses the proc you added (App.proc_DefaultTaxCode) via NodeContextProc wrapper.
                var taxCode = await NodeContext.TaxCodeDefault(description);
                return new JsonResult(new { ok = true, taxCode });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { ok = false, taxCode = string.Empty });
            }
        }

        public async Task<IActionResult> OnPostAsync(string embedded, string returnNode, string taxType, string searchString)
        {
            try
            {
                await LoadListsAsync(taxType);
                await SetViewData();

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError(string.Empty, "Please correct the highlighted fields and try again.");
                    return Page();
                }

                var embeddedMode = string.Equals(embedded, "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(embedded, "true", StringComparison.OrdinalIgnoreCase);

                returnNode = string.IsNullOrWhiteSpace(returnNode) ? "TaxCode" : returnNode;

                App_tbTaxCode.TaxDescription = (App_tbTaxCode.TaxDescription ?? string.Empty).Trim();
                App_tbTaxCode.TaxCode = (App_tbTaxCode.TaxCode ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(App_tbTaxCode.TaxCode) && !string.IsNullOrWhiteSpace(App_tbTaxCode.TaxDescription))
                    App_tbTaxCode.TaxCode = await NodeContext.TaxCodeDefault(App_tbTaxCode.TaxDescription);

                App_tbTaxCode.RoundingCode = await NodeContext.App_tbRoundings
                    .Where(r => r.Rounding == Rounding)
                    .Select(r => r.RoundingCode)
                    .FirstAsync();

                App_tbTaxCode.TaxTypeCode = await NodeContext.App_TaxCodeTypes
                    .Where(t => t.TaxType == TaxType)
                    .Select(t => t.TaxTypeCode)
                    .FirstAsync();

                // UI: 18 => DB: 0.18
                App_tbTaxCode.TaxRate = TaxRatePercent / 100m;

                NodeContext.App_tbTaxCodes.Add(App_tbTaxCode);
                await NodeContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(ReturnUrl))
                    return RedirectToPage(ReturnUrl, new { taxCode = App_tbTaxCode.TaxCode });

                return RedirectToPage("./Index", new
                {
                    embedded = embeddedMode ? "1" : null,
                    returnNode,
                    taxType = string.IsNullOrWhiteSpace(taxType) ? TaxType : taxType,
                    searchString
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task LoadListsAsync(string taxType)
        {
            var roundings = NodeContext.App_tbRoundings.OrderBy(r => r.RoundingCode).Select(r => r.Rounding);
            Roundings = new SelectList(await roundings.ToListAsync());
            Rounding = await roundings.FirstOrDefaultAsync();

            var taxtypes = NodeContext.App_TaxCodeTypes.OrderBy(t => t.TaxTypeCode).Select(t => t.TaxType);
            TaxTypes = new SelectList(await taxtypes.ToListAsync());

            if (!string.IsNullOrWhiteSpace(taxType))
                TaxType = taxType;
            else
                TaxType = await taxtypes.FirstOrDefaultAsync();
        }
    }
}
