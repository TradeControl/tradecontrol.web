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

namespace TradeControl.Web.Pages.Invoice.Register
{
    public class UnpaidDetailModel : DI_BasePageModel
    {
        public UnpaidDetailModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        [BindProperty]
        public Invoice_vwRegisterOverdue Invoice_Detail { get; set; }

        public async Task<IActionResult> OnGetAsync(string invoiceNumber)
        {
            if (invoiceNumber == null)
                return NotFound();

            Invoice_Detail = await NodeContext.Invoice_RegisterOverdue.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

            if (Invoice_Detail == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }
    }
}
