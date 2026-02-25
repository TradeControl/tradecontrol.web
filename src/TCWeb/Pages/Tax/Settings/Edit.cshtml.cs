using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Tax.Settings
{
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Cash_tbTaxType Cash_tbTaxType { get; set; }

        [BindProperty]
        [Display(Name = "Cash Code")]
        public string CashDescription { get; set; }
        public SelectList CashDescriptions { get; set; }

        [BindProperty]
        [Display(Name = "Month")]
        public string MonthName { get; set; }
        public SelectList MonthNames { get; set; }

        [BindProperty]
        [Display(Name = "Recurrence")]
        public string Recurrence { get; set; }
        public SelectList Recurrences { get; set; }

        [BindProperty]
        [Display(Name = "Account")]
        public string SubjectName { get; set; }
        public SelectList SubjectNames { get; set; }

        public async Task<IActionResult> OnGetAsync(short? taxTypeCode)
        {
            try
            {
                if (taxTypeCode == null)
                    return NotFound();

                Cash_tbTaxType = await NodeContext.Cash_tbTaxTypes.FindAsync(taxTypeCode);
                if (Cash_tbTaxType == null)
                    return NotFound();

                var cash_codes = NodeContext.Cash_ExternalCodesLookup.OrderBy(t => t.CashDescription).Select(t => t.CashDescription);
                CashDescriptions = new SelectList(await cash_codes.ToListAsync());
                CashDescription = await NodeContext.Cash_tbCodes.Where(t => t.CashCode == Cash_tbTaxType.CashCode).Select(t => t.CashDescription).FirstAsync();

                var months = NodeContext.App_tbMonths.OrderBy(t => t.MonthNumber).Select(t => t.MonthName);
                MonthNames = new SelectList(await months.ToListAsync());
                MonthName = await NodeContext.App_tbMonths.Where(t => t.MonthNumber == Cash_tbTaxType.MonthNumber).Select(t => t.MonthName).FirstAsync();

                var recurrences = NodeContext.App_tbRecurrences.OrderBy(t => t.RecurrenceCode).Select(t => t.Recurrence);
                Recurrences = new SelectList(await recurrences.ToListAsync());
                Recurrence = await NodeContext.App_tbRecurrences.Where(t => t.RecurrenceCode == Cash_tbTaxType.RecurrenceCode).Select(t => t.Recurrence).FirstAsync();

                var accounts = NodeContext.Subject_SubjectLookup.OrderBy(t => t.SubjectName).Select(t => t.SubjectName);
                SubjectNames = new SelectList(await accounts.ToListAsync());
                SubjectName = await NodeContext.Subject_tbSubjects.Where(t => t.SubjectCode == Cash_tbTaxType.SubjectCode).Select(t => t.SubjectName).FirstAsync();

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
                if (!ModelState.IsValid)
                    return Page();

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? "TaxSettings")
                    : "TaxSettings";

                Cash_tbTaxType.CashCode = await NodeContext.Cash_tbCodes.Where(t => t.CashDescription == CashDescription).Select(t => t.CashCode).FirstAsync();
                Cash_tbTaxType.MonthNumber = await NodeContext.App_tbMonths.Where(t => t.MonthName == MonthName).Select(t => t.MonthNumber).FirstAsync();
                Cash_tbTaxType.RecurrenceCode = await NodeContext.App_tbRecurrences.Where(t => t.Recurrence == Recurrence).Select(t => t.RecurrenceCode).FirstAsync();
                Cash_tbTaxType.SubjectCode = await NodeContext.Subject_tbSubjects.Where(t => t.SubjectName == SubjectName).Select(t => t.SubjectCode).FirstAsync();

                NodeContext.Attach(Cash_tbTaxType).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Cash_tbTaxTypes.AnyAsync(e => e.TaxTypeCode == Cash_tbTaxType.TaxTypeCode))
                        return NotFound();
                    throw;
                }

                return RedirectToPage("./Index",
                    routeValues: new {
                        embedded = embedded ? "1" : null,
                        returnNode
                    });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
