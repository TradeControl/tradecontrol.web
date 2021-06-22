using System;
using System.Collections.Generic;
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

namespace TradeControl.Web.Pages.Invoice.Raise
{
    public class IndexModel : DI_BasePageModel
    {
        public IList<Invoice_vwEntry> Invoice_Entries { get; set; }

        [BindProperty]
        public string InvoiceType { get; set; }
        public SelectList InvoiceTypes { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public IndexModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task OnGetAsync(string invoiceType)
        {
            try
            {
                InvoiceTypes = new SelectList(await NodeContext.Invoice_tbTypes.OrderBy(t => t.InvoiceTypeCode).Select(t => t.InvoiceType).ToListAsync());

                var entries = from tb in NodeContext.Invoice_Entries
                              select tb;

                var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);

                if (!isAuthorized)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    string userId = await profile.UserId(user.Id);

                    entries = from tb in entries
                              where tb.UserId == userId
                              select tb;
                }

                if (!string.IsNullOrEmpty(invoiceType))
                {
                    entries = from tb in entries
                              where tb.InvoiceType == invoiceType
                              select tb;
                }

                Invoice_Entries = await entries.OrderBy(i => i.InvoicedOn).ToListAsync();

                await SetViewData();
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

                Invoices invoices = new(NodeContext);

                var isAuthorized = User.IsInRole(Constants.ManagersRole) || User.IsInRole(Constants.AdministratorsRole);
                bool success = true;
                bool isEmailed = false;


                if (!isAuthorized)
                {
                    var profile = new Profile(NodeContext);
                    var user = await UserManager.GetUserAsync(User);
                    string userId = await profile.UserId(user.Id);
                    isEmailed = await NodeContext.Invoice_Entries.Where(t => t.UserId == userId && (t.InvoiceTypeCode == (short)NodeEnum.InvoiceType.SalesInvoice || t.InvoiceTypeCode == (short)NodeEnum.InvoiceType.CreditNote)).AnyAsync();
                    success = await invoices.Post(userId);
                }
                else
                {
                    List<string> userIds = await NodeContext.Invoice_Entries.Select(t => t.UserId).Distinct().ToListAsync();
                    foreach (string userId in userIds)
                    {
                        if (await NodeContext.Invoice_Entries.Where(t => t.UserId == userId && (t.InvoiceTypeCode == (short)NodeEnum.InvoiceType.SalesInvoice || t.InvoiceTypeCode == (short)NodeEnum.InvoiceType.CreditNote)).AnyAsync())
                            isEmailed = true;

                        if (!await invoices.Post(userId))
                            success = false;
                    }
                }

                if (success)
                {

                    if (isEmailed)
                    {
                        RouteValueDictionary route = new();
                        route.Add("Printed", false);
                        return RedirectToPage("../Update/Index", route);
                    }
                    else
                        return RedirectToPage("./Index");
                }
                else
                    throw new Exception("Unable to raise invoices due to errors.");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
