using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Update
{
    public class EditModel : DI_BasePageModel
    {
        public EditModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        [BindProperty]
        public Invoice_vwRegister Invoice_Header { get; set; }

        [BindProperty]
        public Invoice_tbInvoice Invoice_tbInvoice { get; set; }

        public IList<Invoice_vwRegisterDetail> Invoice_Details { get; set; }

        [BindProperty]
        public string InvoiceType { get; set; }
        public SelectList InvoiceTypes { get; set; }

        [BindProperty]
        public string InvoiceStatus { get; set; }
        public SelectList InvoiceStatuses { get; set; }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber)
        {
            try
            {
                if (invoiceNumber == null)
                    return NotFound();

                Invoice_Header = await NodeContext.Invoice_Register.FirstOrDefaultAsync(m => m.InvoiceNumber == invoiceNumber);

                if (Invoice_Header == null)
                    return NotFound();
                else
                {
                    var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                    if (!isAuthorized)
                    {
                        var profile = new Profile(NodeContext);
                        var user = await UserManager.GetUserAsync(User);
                        string userId = await profile.UserId(user.Id);

                        if (userId != Invoice_Header.UserId)
                            return Forbid();
                    }

                    Invoice_tbInvoice = await NodeContext.Invoice_tbInvoices.FirstAsync(i => i.InvoiceNumber == invoiceNumber);
                    Invoice_Details = await NodeContext.Invoice_RegisterDetails.Where(i => i.InvoiceNumber == invoiceNumber).ToListAsync();

                    InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());
                    InvoiceType = await NodeContext.Invoice_tbTypes.Where(t => t.InvoiceTypeCode == (short)Invoice_tbInvoice.InvoiceTypeCode).Select(t => t.InvoiceType).FirstAsync();

                    InvoiceStatuses = new SelectList(await NodeContext.Invoice_tbStatuses.OrderBy(t => t.InvoiceStatusCode).Select(t => t.InvoiceStatus).ToListAsync());
                    InvoiceStatus = await NodeContext.Invoice_tbStatuses.Where(t => t.InvoiceStatusCode == (short)Invoice_tbInvoice.InvoiceStatusCode).Select(t => t.InvoiceStatus).FirstAsync();

                    await SetViewData();

                    return Page();
                }
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                NodeEnum.InvoiceStatus invoiceStatus = (NodeEnum.InvoiceStatus)await NodeContext.Invoice_tbStatuses
                                                    .Where(t => t.InvoiceStatus == InvoiceStatus)
                                                    .Select(t => t.InvoiceStatusCode).FirstAsync();

                NodeEnum.InvoiceType invoiceType = (NodeEnum.InvoiceType)await NodeContext.Invoice_tbTypes
                                                    .Where(t => t.InvoiceType == InvoiceType)
                                                    .Select(t => t.InvoiceTypeCode).FirstAsync();



                bool periodRebuild = false;

                Periods periods = new(NodeContext);
                DateTime previousInvoicedOn = await NodeContext.Invoice_tbInvoices.Where(i => i.InvoiceNumber == Invoice_tbInvoice.InvoiceNumber)
                                                                                .Select(i => i.InvoicedOn).FirstAsync();
                if (previousInvoicedOn != Invoice_tbInvoice.InvoicedOn)
                {
                    DateTime activePeriod = periods.ActiveStartOn;
                    periodRebuild = (previousInvoicedOn < activePeriod || Invoice_tbInvoice.InvoicedOn < activePeriod);
                }

                bool orgRebuild = ((NodeEnum.InvoiceType)Invoice_tbInvoice.InvoiceTypeCode != invoiceType) || ((NodeEnum.InvoiceStatus)Invoice_tbInvoice.InvoiceStatusCode != invoiceStatus) || periodRebuild;

                Invoice_tbInvoice.InvoiceStatusCode = (short)invoiceStatus;
                Invoice_tbInvoice.InvoiceTypeCode = (short)invoiceType;

                NodeContext.Attach(Invoice_tbInvoice).State = EntityState.Modified;

                try
                {
                    await NodeContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NodeContext.Invoice_tbInvoices.AnyAsync(e => e.AccountCode == Invoice_tbInvoice.InvoiceNumber))
                        return NotFound();
                    else
                    {
                        NodeContext.ErrorLog(new DbUpdateConcurrencyException());
                        throw;
                    }
                }

                Invoices invoices = new(NodeContext, Invoice_tbInvoice.InvoiceNumber);

                if (await invoices.Accept())
                {
                    if (periodRebuild)
                        periodRebuild = await periods.Generate();

                    if (orgRebuild)
                    {
                        Orgs orgs = new(NodeContext, Invoice_tbInvoice.AccountCode);
                        orgRebuild = await orgs.Rebuild();
                    }
                }

                RouteValueDictionary route = new();
                route.Add("InvoiceNumber", Invoice_tbInvoice.InvoiceNumber);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
