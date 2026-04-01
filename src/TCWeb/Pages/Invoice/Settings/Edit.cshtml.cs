using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Settings
{
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Invoice_tbType Invoice_Type { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Embedded { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnNode { get; set; }

        public async Task<IActionResult> OnGetAsync(short? invoiceTypeCode)
        {
            if (invoiceTypeCode == null)
                return NotFound();

            Invoice_Type = await NodeContext.Invoice_tbTypes.FirstOrDefaultAsync(m => m.InvoiceTypeCode == invoiceTypeCode);

            if (Invoice_Type == null)
                return NotFound();

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetViewData();
                    return Page();
                }

                NodeContext.Attach(Invoice_Type).State = EntityState.Modified;
                await NodeContext.SaveChangesAsync();

                return RedirectToPage("./Index", new {
                    embedded = string.Equals(Embedded, "1", StringComparison.OrdinalIgnoreCase) ? "1" : null,
                    returnNode = string.IsNullOrWhiteSpace(ReturnNode) ? null : ReturnNode
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
