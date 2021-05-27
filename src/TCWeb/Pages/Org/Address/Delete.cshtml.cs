using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Org.Address
{
    [Authorize(Roles = "Administrators, Managers")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        [BindProperty]
        public Org_vwAddressList Address { get; set; }

        public async Task<IActionResult> OnGetAsync(string addressCode)
        {
            if (addressCode == null)
                return NotFound();

            Address = await NodeContext.Org_AddressList.FirstOrDefaultAsync(m => m.AddressCode == addressCode);

            if (Address == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAsync(string addressCode)
        {
            if (addressCode == null)
                return NotFound();

            Org_tbAddress tbAddress = await NodeContext.Org_tbAddresses.FindAsync(addressCode);

            if (tbAddress != null)
            {
                
                NodeContext.Org_tbAddresses.Remove(tbAddress);
                await NodeContext.SaveChangesAsync();
            }

            RouteValueDictionary route = new();
            route.Add("accountCode", tbAddress.AccountCode);

            return RedirectToPage("./Index", route);
        }
    }
}
